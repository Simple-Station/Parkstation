using System.Collections.Immutable;
using System.Linq;
using Content.Server.Database;
using Content.Server.Chat.Managers;
using Content.Server.Players;
using Content.Server.SimpleStation14.Minor;
using Content.Shared.Mobs.Systems;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.Objectives.Interfaces;

namespace Content.Server.GameTicking.Rules;

public sealed class MinorRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IObjectivesManager _objectivesManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IServerDbManager _db = default!;


    private readonly SoundSpecifier _addedSound = new SoundPathSpecifier("/Audio/Misc/tatoralert.ogg");
    public List<MinorRole> Minors = new();

    public override string Prototype => "Minor";
    private const string MinorPrototypeID = "Minor";

    public int TotalMinors => Minors.Count;

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

    public override void Started() { }

    public override void Ended()
    {
        Minors.Clear();
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        if (!RuleAdded) return;

        var minPlayers = _cfg.GetCVar(CCVars.MinorMinPlayers);
        if (!ev.Forced && ev.Players.Length < minPlayers)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("preset-minor-not-enough-ready-players", ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
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

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        if (!RuleAdded) return;
        if (cont == false) return;

        var numMinors = MathHelper.Clamp(ev.Players.Length / _playersPerMinor, 1, _maxMinors);

        var minorPool = FindPotentialMinors(ev);
        var selectedMinors = PickMinors(numMinors, minorPool);

        foreach (var minor in selectedMinors) MakeMinor(minor);
    }

    public List<IPlayerSession> FindPotentialMinors(RulePlayerJobsAssignedEvent ev)
    {
        var list = new List<IPlayerSession>(ev.Players).Where(x =>
            x.Data.ContentData()?.Mind?.AllRoles.All(role => role is not Content.Server.Roles.Job { CanBeAntag: false }) ?? false
        ).ToList();

        var prefList = new List<IPlayerSession>();

        foreach (var player in list)
        {
            if (!ev.Profiles.ContainsKey(player.UserId))
            {
                continue;
            }

            var profile = ev.Profiles[player.UserId];
            if (profile.AntagPreferences.Contains(MinorPrototypeID))
            {
                prefList.Add(player);
            }
        }
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient preferred minors, picking at random.");
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
        Logger.InfoS("preset", $"Making {minor.ConnectedClient.UserName} a minor.");
        var mind = minor.Data.ContentData()?.Mind;
        if (mind == null)
        {
            Logger.ErrorS("preset", "Failed getting mind for picked minor.");
            return;
        }

        if (_cfg.GetCVar(CCVars.WhitelistEnabled) && !await _db.GetWhitelistStatusAsync(minor.UserId))
        {
            Logger.ErrorS("preset", "Selected minor is not whitelisted, preventing their selection.");
            return;
        }

        var antagPrototype = _prototypeManager.Index<AntagPrototype>(MinorPrototypeID);
        var minorRole = new MinorRole(mind, antagPrototype);
        mind.AddRole(minorRole);
        Minors.Add(minorRole);
        minorRole.GreetMinor();

        var maxDifficulty = _cfg.GetCVar(CCVars.MinorMaxDifficulty);
        var maxPicks = _cfg.GetCVar(CCVars.MinorMaxPicks);

        // give minor antag their objective
        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectivesManager.GetRandomObjective(minorRole.Mind, "MinorantagObjectiveGroup");
            if (objective == null) continue;
            if (minorRole.Mind.TryAddObjective(objective)) difficulty += objective.Difficulty;
        }

        SoundSystem.Play(_addedSound.GetSound(), Filter.Empty().AddPlayer(minor), AudioParams.Default);
        Logger.InfoS("preset", $"Made {minor.ConnectedClient.UserName} a minor.");
        return;
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        if (!RuleAdded)
            return;
        if (TotalMinors >= _maxMinors)
            return;
        if (!ev.LateJoin)
            return;
        if (!ev.Profile.AntagPreferences.Contains(MinorPrototypeID))
            return;


        if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
            return;

        if (!job.CanBeAntag)
            return;

        // the nth player we adjust our probabilities around
        int target = ((_playersPerMinor * TotalMinors) + 1);

        float chance = (1f / _playersPerMinor);

        /// If we have too many minors, divide by how many players below target for next minor we are.
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

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        var result = Loc.GetString("minor-round-end-result", ("minorCount", Minors.Count));

        foreach (var minor in Minors)
        {
            // var name = minor.Mind.CharacterName;
            // minor.Mind.TryGetSession(out var session);
            // var username = session?.Name;

            // result += $"\n- {name}, {username} was a minor antagonist, their objective was; '{minor.Mind.Briefing}'";

            var name = minor.Mind.CharacterName;
            minor.Mind.TryGetSession(out var session);
            var username = session?.Name;

            var objectives = minor.Mind.AllObjectives.ToArray();
            if (objectives.Length == 0)
            {
                if (username != null)
                {
                    if (name == null)
                        result += "\n" + Loc.GetString("minor-user-was-a-minor", ("user", username));
                    else
                        result += "\n" + Loc.GetString("minor-user-was-a-minor-named", ("user", username), ("name", name));
                }
                else if (name != null)
                    result += "\n" + Loc.GetString("minor-was-a-minor-named", ("name", name));

                continue;
            }

            if (username != null)
            {
                if (name == null)
                    result += "\n" + Loc.GetString("minor-user-was-a-minor-with-objectives", ("user", username));
                else
                    result += "\n" + Loc.GetString("minor-user-was-a-minor-with-objectives-named", ("user", username), ("name", name));
            }
            else if (name != null)
                result += "\n" + Loc.GetString("minor-was-a-minor-with-objectives-named", ("name", name));

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

    public IEnumerable<MinorRole> GetOtherMinorsAliveAndConnected(Mind.Mind ourMind)
    {
        var minors = Minors;
        List<MinorRole> removeList = new();

        return Minors // don't want
            .Where(t => t.Mind is not null) // no mind
            .Where(t => t.Mind.OwnedEntity is not null) // no entity
            .Where(t => t.Mind.Session is not null) // player disconnected
            .Where(t => t.Mind != ourMind) // ourselves
            .Where(t => _mobStateSystem.IsAlive((EntityUid) t.Mind.OwnedEntity!)) // dead
            .Where(t => t.Mind.CurrentEntity == t.Mind.OwnedEntity); // not in original body
    }
}
