using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Objectives.Interfaces;
using Content.Server.Players;
using Content.Server.Shuttles.Components;
using Content.Server.SimpleStation14.Antagonists.Rules.Components;
using Content.Server.SimpleStation14.Minor;
using Content.Shared.CCVar;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Antagonists.Rules;

public sealed class MinorRuleSystem : GameRuleSystem<MinorRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IObjectivesManager _objectivesManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    private ISawmill _sawmill = default!;


    private int _playersPerMinor => _cfg.GetCVar(CCVars.MinorPlayersPerMinor);
    private int _maxMinors => _cfg.GetCVar(CCVars.MinorMaxMinors);

    private bool cont = true;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayersSpawned);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLatejoin);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        var query = EntityQueryEnumerator<MinorRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var minor, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var minPlayers = _cfg.GetCVar(CCVars.MinorMinPlayers);
            if (!ev.Forced && ev.Players.Length < minPlayers)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("preset-minor-not-enough-ready-players",
                    ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
                cont = false;
                return;
            }

            if (ev.Players.Length == 0)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("preset-minor-no-one-ready"));
                cont = false;
                return;
            }
        }
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        var query = EntityQueryEnumerator<MinorRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var minor, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            foreach (var player in ev.Players)
            {
                if (!ev.Profiles.ContainsKey(player.UserId))
                    continue;

                minor.StartCandidates[player] = ev.Profiles[player.UserId];
            }

            var delay = TimeSpan.FromSeconds(
                _cfg.GetCVar(CCVars.TraitorStartDelay) +
                _random.NextFloat(0f, _cfg.GetCVar(CCVars.TraitorStartDelayVariance)));

            minor.AnnounceAt = _gameTiming.CurTime + delay;

            minor.SelectionStatus = MinorRuleComponent.SelectionState.ReadyToSelect;
        }
    }

    public List<IPlayerSession> FindPotentialTraitors(in Dictionary<IPlayerSession, HumanoidCharacterProfile> candidates, TraitorRuleComponent component)
    {
        var list = new List<IPlayerSession>();
        var pendingQuery = GetEntityQuery<PendingClockInComponent>();

        foreach (var player in candidates.Keys)
        {
            // Role prevents antag.
            if (!(player.Data.ContentData()?.Mind?.AllRoles.All(role => role is not Roles.Job { CanBeAntag: false }) ?? false))
            {
                continue;
            }

            // Latejoin
            if (player.AttachedEntity != null && pendingQuery.HasComponent(player.AttachedEntity.Value))
                continue;

            list.Add(player);
        }

        var prefList = new List<IPlayerSession>();

        foreach (var player in list)
        {
            var profile = candidates[player];
            if (profile.AntagPreferences.Contains(component.TraitorPrototypeId))
            {
                prefList.Add(player);
            }
        }
        if (prefList.Count == 0)
        {
            _sawmill.Info("Insufficient preferred minor antags, picking at random.");
            prefList = list;
        }
        return prefList;
    }

    public List<IPlayerSession> PickMinors(int minorCount, List<IPlayerSession> prefList)
    {
        var results = new List<IPlayerSession>(minorCount);
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient ready players to fill up with minors, stopping the selection.");
            return results;
        }

        for (var i = 0; i < minorCount; i++)
        {
            var minor = _random.PickAndTake(prefList);
            results.Add(minor);
            Logger.InfoS("preset", $"Selected {minor.ConnectedClient.UserName} as a minor.");
        }
        return results;
    }

    public async void MakeMinor(IPlayerSession minor)
    {
        var minorRule = EntityQuery<MinorRuleComponent>().FirstOrDefault();
        if (minorRule == null)
        {
            GameTicker.StartGameRule("Minor", out var ruleEntity);
            minorRule = EntityManager.GetComponent<MinorRuleComponent>(ruleEntity);
        }

        var mind = minor.Data.ContentData()?.Mind;
        if (mind == null)
        {
            _sawmill.Info("Failed getting mind for picked traitor.");
            return;
        }

        if (_cfg.GetCVar(CCVars.WhitelistEnabled))
        {
            if (minor.ContentData == null || !minor.ContentData()!.Whitelisted)
                return;
        }

        if (mind.OwnedEntity is not { } entity)
        {
            Logger.ErrorS("preset", "Mind picked for traitor did not have an attached entity.");
            return;
        }

        var antagPrototype = _prototypeManager.Index<AntagPrototype>(minorRule.MinorPrototypeId);
        var minorRole = new MinorRole(mind, antagPrototype);
        mind.AddRole(minorRole);
        minorRule.Minors.Add(minorRole);
        minorRole.GreetMinor();

        var maxDifficulty = _cfg.GetCVar(CCVars.MinorMaxDifficulty);
        var maxPicks = _cfg.GetCVar(CCVars.MinorMaxPicks);

        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectivesManager.GetRandomObjective(minorRole.Mind, "TraitorObjectiveGroups");
            if (objective == null) continue;
            if (minorRole.Mind.TryAddObjective(objective))
                difficulty += objective.Difficulty;
        }

        _audioSystem.PlayGlobal(minorRule.AddedSound, Filter.Empty().AddPlayer(minor), false, AudioParams.Default);
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<MinorRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var minor, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;
            if (minor.TotalMinors >= _maxMinors)
                return;
            if (!ev.LateJoin)
                return;
            if (!ev.Profile.AntagPreferences.Contains(minor.MinorPrototypeId))
                return;


            if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
                return;

            if (!job.CanBeAntag)
                return;

            // the nth player we adjust our probabilities around
            var target = ((_playersPerMinor * minor.TotalMinors) + 1);

            var chance = (1f / _playersPerMinor);

            // If we have too many minors, divide by how many players below target for next minor we are.
            if (ev.JoinOrder < target)
            {
                chance /= (target - ev.JoinOrder);
            }
            else // Tick up towards 100% chance.
            {
                chance *= ((ev.JoinOrder + 1) - target);
            }

            if (chance > 1)
                chance = 1;

            // Now that we've calculated our chance, roll and make them a minor if we roll under.
            // You get one shot.
            if (_random.Prob((float) chance))
            {
                MakeMinor(ev.Player);
            }
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var query = EntityQueryEnumerator<MinorRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var minor, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var result = Loc.GetString("minor-round-end-result", ("minorCount", minor.Minors.Count));

            foreach (var m in minor.Minors)
            {
                var name = m.Mind.CharacterName;
                m.Mind.TryGetSession(out var session);
                var username = session?.Name;

                var objectives = m.Mind.AllObjectives.ToArray();
                if (objectives.Length == 0)
                {
                    if (username != null)
                    {
                        if (name == null)
                        {
                            result += "\n" + Loc.GetString("minor-user-was-a-minor", ("user", username));
                        }
                        else
                        {
                            result += "\n" + Loc.GetString("minor-user-was-a-minor-named", ("user", username),
                                ("name", name));
                        }
                    }
                    else if (name != null)
                    {
                        result += "\n" + Loc.GetString("minor-was-a-minor-named", ("name", name));
                    }

                    continue;
                }

                if (username != null)
                {
                    if (name == null)
                    {
                        result += "\n" + Loc.GetString("minor-user-was-a-minor-with-objectives", ("user", username));
                    }
                    else
                    {
                        result += "\n" + Loc.GetString("minor-user-was-a-minor-with-objectives-named",
                            ("user", username), ("name", name));
                    }
                }
                else if (name != null)
                {
                    result += "\n" + Loc.GetString("minor-was-a-minor-with-objectives-named", ("name", name));
                }

                foreach (var objectiveGroup in objectives.GroupBy(o => o.Prototype.Issuer))
                {
                    result += "\n" + Loc.GetString($"preset-minor-objective-issuer-{objectiveGroup.Key}");

                    foreach (var objective in objectiveGroup)
                    {
                        foreach (var condition in objective.Conditions)
                        {
                            result += "\n- " + Loc.GetString(
                                "minor-objective-condition-success",
                                ("condition", condition.Title),
                                ("markupColor", "green")
                            );
                        }
                    }
                }
            }

            ev.AddLine(result);
        }
    }

    public List<MinorRole> GetOtherTraitorsAliveAndConnected(Mind.Mind ourMind, MinorRuleComponent component)
    {
        return component.Minors // don't want
            .Where(t => t.Mind.OwnedEntity is not null) // no entity
            .Where(t => t.Mind.Session is not null) // player disconnected
            .Where(t => t.Mind != ourMind) // ourselves
            .Where(t => _mobStateSystem.IsAlive((EntityUid) t.Mind.OwnedEntity!)) // dead
            .Where(t => t.Mind.CurrentEntity == t.Mind.OwnedEntity).ToList(); // not in original body
    }
}
