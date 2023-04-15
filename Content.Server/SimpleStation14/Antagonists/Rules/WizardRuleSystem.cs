using System.Collections.Immutable;
using System.Linq;
using Content.Server.Database;
using Content.Server.Chat.Managers;
using Content.Server.Objectives.Interfaces;
using Content.Server.Players;
using Content.Server.Traitor.Uplink;
using Content.Server.SimpleStation14.Wizard;
using Content.Shared.Mobs.Systems;
using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

public sealed class WizardRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IObjectivesManager _objectivesManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly IServerDbManager _db = default!;


    public override string Prototype => "Wizard";

    private readonly SoundSpecifier _addedSound = new SoundPathSpecifier("/Audio/Misc/tatoralert.ogg");
    public List<WizardRole> Wizards = new();

    private const string WizardPrototypeID = "Wizard";
    private const string WizardUplinkPresetId = "WizardStorePresetUplink";

    public int TotalWizards => Wizards.Count;
    public string[] Codewords = new string[3];

    private int _playersPerWizard => _cfg.GetCVar(CCVars.WizardPlayersPerWizard);
    private int _maxWizards => _cfg.GetCVar(CCVars.WizardMaxWizards);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayersSpawned);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLatejoin);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    public override void Started(){}

    public override void Ended()
    {
        Wizards.Clear();
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        MakeCodewords();
        if (!RuleAdded) return;

        var minPlayers = _cfg.GetCVar(CCVars.WizardMinPlayers);
        if (!ev.Forced && ev.Players.Length < minPlayers)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("preset-wizard-not-enough-ready-players", ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
            ev.Cancel();
            return;
        }

        if (ev.Players.Length == 0)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("preset-wizard-no-one-ready"));
            ev.Cancel();
            return;
        }
    }

    private void MakeCodewords()
    {

        var codewordCount = _cfg.GetCVar(CCVars.WizardCodewordCount);
        var adjectives = _prototypeManager.Index<DatasetPrototype>("adjectives").Values;
        var verbs = _prototypeManager.Index<DatasetPrototype>("verbs").Values;
        var codewordPool = adjectives.Concat(verbs).ToList();
        var finalCodewordCount = Math.Min(codewordCount, codewordPool.Count);
        Codewords = new string[finalCodewordCount];
        for (var i = 0; i < finalCodewordCount; i++)
        {
            Codewords[i] = _random.PickAndTake(codewordPool);
        }
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        if (!RuleAdded)
            return;

        var numWizards = MathHelper.Clamp(ev.Players.Length / _playersPerWizard, 1, _maxWizards);
        var codewordCount = _cfg.GetCVar(CCVars.WizardCodewordCount);

        var wizardPool = FindPotentialWizards(ev);
        var selectedWizards = PickWizards(numWizards, wizardPool);

        foreach (var wizard in selectedWizards)
            MakeWizard(wizard);
    }

    public List<IPlayerSession> FindPotentialWizards(RulePlayerJobsAssignedEvent ev)
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
            if (profile.AntagPreferences.Contains(WizardPrototypeID))
            {
                prefList.Add(player);
            }
        }
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient preferred wizards, picking at random.");
            prefList = list;
        }
        return prefList;
    }

    public List<IPlayerSession> PickWizards(int wizardCount, List<IPlayerSession> prefList)
    {
        var results = new List<IPlayerSession>(wizardCount);
        if (prefList.Count == 0)
        {
            Logger.InfoS("preset", "Insufficient ready players to fill up with wizards, stopping the selection.");
            return results;
        }

        for (var i = 0; i < wizardCount; i++)
        {
            var wizard = _random.PickAndTake(prefList);
            results.Add(wizard);
            Logger.InfoS("preset", $"Selected {wizard.ConnectedClient.UserName} as a wizard.");
        }
        return results;
    }

    public async void MakeWizard(IPlayerSession wizard)
    {
        Logger.InfoS("preset", $"Making {wizard.ConnectedClient.UserName} a wizard.");
        var mind = wizard.Data.ContentData()?.Mind;
        if (mind == null)
        {
            Logger.ErrorS("preset", $"Failed getting mind for {wizard.ConnectedClient.UserName}.");
            return;
        }

        if (_cfg.GetCVar(CCVars.WhitelistEnabled) && !await _db.GetWhitelistStatusAsync(wizard.UserId))
        {
            Logger.ErrorS("preset", $"{wizard.ConnectedClient.UserName} is not whitelisted, preventing their selection.");
            return;
        }

        // create uplink for the antag.
        // PDA should be in place already
        DebugTools.AssertNotNull(mind.OwnedEntity);

        var startingBalance = _cfg.GetCVar(CCVars.WizardStartingBalance);

        if (mind.CurrentJob != null) startingBalance = Math.Max(startingBalance - mind.CurrentJob.Prototype.AntagAdvantage, 0);

        if (!_uplink.AddUplink(mind.OwnedEntity!.Value, startingBalance, WizardUplinkPresetId)) return;

        var antagPrototype = _prototypeManager.Index<AntagPrototype>(WizardPrototypeID);
        var wizardRole = new WizardRole(mind, antagPrototype);
        mind.AddRole(wizardRole);
        Wizards.Add(wizardRole);
        wizardRole.GreetWizard(Codewords);

        var maxDifficulty = _cfg.GetCVar(CCVars.WizardMaxDifficulty);
        var maxPicks = _cfg.GetCVar(CCVars.WizardMaxPicks);

        //give wizards their objectives
        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectivesManager.GetRandomObjective(wizardRole.Mind, "WizardObjectiveGroups");
            if (objective == null) continue;
            if (wizardRole.Mind.TryAddObjective(objective))
                difficulty += objective.Difficulty;
        }

        //give wizards their codewords to keep in their character info menu
        wizardRole.Mind.Briefing = Loc.GetString("wizard-role-codewords", ("codewords", string.Join(", ", Codewords)));

        SoundSystem.Play(_addedSound.GetSound(), Filter.Empty().AddPlayer(wizard), AudioParams.Default);
        Logger.InfoS("preset", $"Made {wizard.ConnectedClient.UserName} a wizard.");
        return;
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        if (!RuleAdded)
            return;
        if (TotalWizards >= _maxWizards)
            return;
        if (!ev.LateJoin)
            return;
        if (!ev.Profile.AntagPreferences.Contains(WizardPrototypeID))
            return;


        if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
            return;

        if (!job.CanBeAntag)
            return;

        // the nth player we adjust our probabilities around
        int target = ((_playersPerWizard * TotalWizards) + 1);

        float chance = (1f / _playersPerWizard);

        /// If we have too many wizards, divide by how many players below target for next wizard we are.
        if (ev.JoinOrder < target)
        {
            chance /= (target - ev.JoinOrder);
        } else // Tick up towards 100% chance.
        {
            chance *= ((ev.JoinOrder + 1) - target);
        }
        if (chance > 1)
            chance = 1;

        // Now that we've calculated our chance, roll and make them a wizard if we roll under.
        // You get one shot.
        if (_random.Prob((float) chance))
        {
            MakeWizard(ev.Player);
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        var result = Loc.GetString("wizard-round-end-result", ("wizardCount", Wizards.Count));

        foreach (var wizard in Wizards)
        {
            var name = wizard.Mind.CharacterName;
            wizard.Mind.TryGetSession(out var session);
            var username = session?.Name;

            var objectives = wizard.Mind.AllObjectives.ToArray();
            if (objectives.Length == 0)
            {
                if (username != null)
                {
                    if (name == null)
                        result += "\n" + Loc.GetString("wizard-user-was-a-wizard", ("user", username));
                    else
                        result += "\n" + Loc.GetString("wizard-user-was-a-wizard-named", ("user", username), ("name", name));
                }
                else if (name != null)
                    result += "\n" + Loc.GetString("wizard-was-a-wizard-named", ("name", name));

                continue;
            }

            if (username != null)
            {
                if (name == null)
                    result += "\n" + Loc.GetString("wizard-user-was-a-wizard-with-objectives", ("user", username));
                else
                    result += "\n" + Loc.GetString("wizard-user-was-a-wizard-with-objectives-named", ("user", username), ("name", name));
            }
            else if (name != null)
                result += "\n" + Loc.GetString("wizard-was-a-wizard-with-objectives-named", ("name", name));

            foreach (var objectiveGroup in objectives.GroupBy(o => o.Prototype.Issuer))
            {
                result += "\n" + Loc.GetString($"preset-wizard-objective-issuer-{objectiveGroup.Key}");

                foreach (var objective in objectiveGroup)
                {
                    foreach (var condition in objective.Conditions)
                    {
                        var progress = condition.Progress;
                        if (progress > 0.99f)
                        {
                            result += "\n- " + Loc.GetString(
                                "wizard-objective-condition-success",
                                ("condition", condition.Title),
                                ("markupColor", "green")
                            );
                        }
                        else
                        {
                            result += "\n- " + Loc.GetString(
                                "wizard-objective-condition-fail",
                                ("condition", condition.Title),
                                ("progress", (int) (progress * 100)),
                                ("markupColor", "red")
                            );
                        }
                    }
                }
            }
        }
        ev.AddLine(result);
    }

    public IEnumerable<WizardRole> GetOtherWizardsAliveAndConnected(Mind.Mind ourMind)
    {
        var wizards = Wizards;
        List<WizardRole> removeList = new();

        return Wizards // don't want
            .Where(t => t.Mind is not null) // no mind
            .Where(t => t.Mind.OwnedEntity is not null) // no entity
            .Where(t => t.Mind.Session is not null) // player disconnected
            .Where(t => t.Mind != ourMind) // ourselves
            .Where(t => _mobStateSystem.IsAlive((EntityUid) t.Mind.OwnedEntity!)) // dead
            .Where(t => t.Mind.CurrentEntity == t.Mind.OwnedEntity); // not in original body
    }
}
