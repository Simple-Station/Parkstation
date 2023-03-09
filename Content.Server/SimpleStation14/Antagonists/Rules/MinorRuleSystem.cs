using System.Collections.Immutable;
using System.Linq;
using Content.Server.Database;
using Content.Server.Chat.Managers;
using Content.Server.Players;
using Content.Server.SimpleStation14.Flawed;
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

public sealed class FlawedRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IObjectivesManager _objectivesManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;


    private readonly SoundSpecifier _addedSound = new SoundPathSpecifier("/Audio/Animals/pig_oink.ogg");
    public List<FlawedRole> Flawed = new();

    public override string Prototype => "Flawed";
    private const string FlawedPrototypeID = "Flawed";

    public int TotalFlawed => Flawed.Count;

    private int _playersPerFlawed => _cfg.GetCVar(CCVars.FlawedPlayersPerFlawed);
    private int _maxFlawed => _cfg.GetCVar(CCVars.FlawedMaxFlawed);

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
        Flawed.Clear();
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        if (!RuleAdded) return;

        var minPlayers = _cfg.GetCVar(CCVars.FlawedMinPlayers);
        if (!ev.Forced && ev.Players.Length < minPlayers)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("flawed-not-enough-ready-players", ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
            cont = false;
            return;
        }

        if (ev.Players.Length == 0)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("flawed-no-one-ready"));
            cont = false;
            return;
        }
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        if (!RuleAdded) return;
        if (cont == false) return;

        var numFlawed = MathHelper.Clamp(ev.Players.Length / _playersPerFlawed, 1, _maxFlawed);

        var flawedPool = FindPotentialFlawed(ev);
        var selectedFlawed = PickFlawed(numFlawed, flawedPool);

        foreach (var flawed in selectedFlawed) MakeFlawed(flawed);
    }

    public List<IPlayerSession> FindPotentialFlawed(RulePlayerJobsAssignedEvent ev)
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
            if (profile.AntagPreferences.Contains(FlawedPrototypeID))
            {
                prefList.Add(player);
            }
        }
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient preferred flawed, picking at random.");
            prefList = list;
        }
        return prefList;
    }

    public List<IPlayerSession> PickFlawed(int flawedCount, List<IPlayerSession> prefList)
    {
        var results = new List<IPlayerSession>(flawedCount);
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient ready players to fill up with flawed, stopping the selection.");
            return results;
        }

        for (var i = 0; i < flawedCount; i++)
        {
            var flawed = _random.PickAndTake(prefList);
            results.Add(flawed);
            Logger.InfoS("preset", $"Selected {flawed.ConnectedClient.UserName} as a flawed.");
        }
        return results;
    }

    public async void MakeFlawed(IPlayerSession flawed)
    {
        Logger.InfoS("preset", $"Making {flawed.ConnectedClient.UserName} a flawed.");
        var mind = flawed.Data.ContentData()?.Mind;
        if (mind == null)
        {
            Logger.ErrorS("preset", "Failed getting mind for picked flawed.");
            return;
        }

        if (_cfg.GetCVar(CCVars.WhitelistEnabled) && !await _db.GetWhitelistStatusAsync(flawed.UserId))
        {
            Logger.ErrorS("preset", "Selected flawed is not whitelisted, preventing their selection.");
            return;
        }

        var antagPrototype = _prototypeManager.Index<AntagPrototype>(FlawedPrototypeID);
        var flawedRole = new FlawedRole(mind, antagPrototype);
        mind.AddRole(flawedRole);
        Flawed.Add(flawedRole);
        flawedRole.GreetFlawed();

        var maxDifficulty = _cfg.GetCVar(CCVars.FlawedMaxDifficulty);
        var maxPicks = _cfg.GetCVar(CCVars.FlawedMaxPicks);

        // give flawed antag their objective
        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectivesManager.GetRandomObjective(flawedRole.Mind, "FlawedObjectiveGroups");
            if (objective == null) continue;
            if (flawedRole.Mind.TryAddObjective(objective)) difficulty += objective.Difficulty;
        }

        _audioSystem.PlayGlobal(_audioSystem.GetSound(_addedSound), flawed, AudioParams.Default);
        Logger.InfoS("preset", $"Made {flawed.ConnectedClient.UserName} a flawed.");
        return;
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        if (!RuleAdded)
            return;
        if (TotalFlawed >= _maxFlawed)
            return;
        if (!ev.LateJoin)
            return;
        if (!ev.Profile.AntagPreferences.Contains(FlawedPrototypeID))
            return;


        if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
            return;

        if (!job.CanBeAntag)
            return;

        // the nth player we adjust our probabilities around
        int target = ((_playersPerFlawed * TotalFlawed) + 1);

        float chance = (1f / _playersPerFlawed);

        /// If we have too many flawed, divide by how many players below target for next flawed we are.
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

        // Now that we've calculated our chance, roll and make them a flawed if we roll under.
        // You get one shot.
        if (_random.Prob((float) chance))
        {
            MakeFlawed(ev.Player);
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        var result = Loc.GetString("flawed-round-end-result", ("flawedCount", Flawed.Count));

        foreach (var flawed in Flawed)
        {
            // var name = flawed.Mind.CharacterName;
            // flawed.Mind.TryGetSession(out var session);
            // var username = session?.Name;

            // result += $"\n- {name}, {username} was a flawed antagonist, their objective was; '{flawed.Mind.Briefing}'";

            var name = flawed.Mind.CharacterName;
            flawed.Mind.TryGetSession(out var session);
            var username = session?.Name;

            var objectives = flawed.Mind.AllObjectives.ToArray();
            if (objectives.Length == 0)
            {
                if (username != null)
                {
                    if (name == null)
                        result += "\n" + Loc.GetString("flawed-user-was-a-flawed", ("user", username));
                    else
                        result += "\n" + Loc.GetString("flawed-user-was-a-flawed-named", ("user", username), ("name", name));
                }
                else if (name != null)
                    result += "\n" + Loc.GetString("flawed-was-a-flawed-named", ("name", name));

                continue;
            }

            if (username != null)
            {
                if (name == null)
                    result += "\n" + Loc.GetString("flawed-user-was-a-flawed-with-objectives", ("user", username));
                else
                    result += "\n" + Loc.GetString("flawed-user-was-a-flawed-with-objectives-named", ("user", username), ("name", name));
            }
            else if (name != null)
                result += "\n" + Loc.GetString("flawed-was-a-flawed-with-objectives-named", ("name", name));

            foreach (var objectiveGroup in objectives.GroupBy(o => o.Prototype.Issuer))
            {
                result += "\n" + Loc.GetString($"preset-flawed-objective-issuer-{objectiveGroup.Key}");

                foreach (var objective in objectiveGroup)
                {
                    foreach (var condition in objective.Conditions)
                    {
                        result += "\n- " + Loc.GetString(
                            "flawed-objective-condition-success",
                            ("condition", condition.Title),
                            ("markupColor", "green")
                        );
                    }
                }
            }
        }
        ev.AddLine(result);
    }

    public IEnumerable<FlawedRole> GetOtherFlawedAliveAndConnected(Mind.Mind ourMind)
    {
        var flawed = Flawed;
        List<FlawedRole> removeList = new();

        return Flawed // don't want
            .Where(t => t.Mind is not null) // no mind
            .Where(t => t.Mind.OwnedEntity is not null) // no entity
            .Where(t => t.Mind.Session is not null) // player disconnected
            .Where(t => t.Mind != ourMind) // ourselves
            .Where(t => _mobStateSystem.IsAlive((EntityUid) t.Mind.OwnedEntity!)) // dead
            .Where(t => t.Mind.CurrentEntity == t.Mind.OwnedEntity); // not in original body
    }
}
