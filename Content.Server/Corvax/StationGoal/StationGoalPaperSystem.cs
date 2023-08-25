using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Fax;
using Content.Server.Station.Systems;
using Content.Shared.Corvax.CCCVars;
using Content.Shared.GameTicking;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Corvax.StationGoal;

/// <summary>
///     System to spawn paper with station goal.
/// </summary>
public sealed class StationGoalPaperSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FaxSystem _faxSystem = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private static readonly Regex StationIdRegex = new(@".*-(\d+)$");

    private const string RandomPrototype = "StationGoals";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        if (_config.GetCVar(CCCVars.StationGoalsEnabled) != true)
            return;

        SendRandomGoal();
    }

    /// <summary>
    ///     Send a random station goal to all faxes which are authorized to receive it
    /// </summary>
    /// <returns>If the fax was successful</returns>
    /// <exception cref="ConstraintException">Raised when station goal types in the prototype is invalid</exception>
    public bool SendRandomGoal()
    {
        // Get the random station goal list
        if (!_prototypeManager.TryIndex<WeightedRandomPrototype>(RandomPrototype, out var goals))
            return false;

        // Get a random goal
        var goal = RecursiveRandom(goals);

        // Send the goal
        return SendStationGoal(goal);
    }

    private StationGoalPrototype RecursiveRandom(WeightedRandomPrototype random)
    {
        var goal = random.Pick(_random);

        if (_prototypeManager.TryIndex<StationGoalPrototype>(goal, out var goalPrototype))
        {
            return goalPrototype;
        }

        if (_prototypeManager.TryIndex<WeightedRandomPrototype>(goal, out var goalRandom))
        {
            return RecursiveRandom(goalRandom);
        }

        throw new Exception($"StationGoalPaperSystem: Random station goal could not be found from origin prototype {RandomPrototype}");
    }

    /// <summary>
    ///     Send a station goal to all faxes which are authorized to receive it
    /// </summary>
    /// <returns>True if at least one fax received paper</returns>
    public bool SendStationGoal(StationGoalPrototype goal)
    {
        var enumerator = EntityManager.EntityQueryEnumerator<FaxMachineComponent>();
        var wasSent = false;

        while (enumerator.MoveNext(out var uid, out var fax))
        {
            if (!fax.ReceiveStationGoal)
                continue;

            if (!TryComp<MetaDataComponent>(_station.GetOwningStation(uid), out var meta))
                continue;

            var stationId = StationIdRegex.Match(meta.EntityName).Groups[1].Value;

            var printout = new FaxPrintout(
                Loc.GetString(goal.Text,
                    ("date", DateTime.Now.AddYears(1000).ToString("yyyy MMMM dd")),
                    ("station", string.IsNullOrEmpty(stationId) ? "???" : stationId)
                ),
                Loc.GetString("station-goal-fax-paper-name"),
                "StationGoalPaper"
            );

            _faxSystem.Receive(uid, printout, null, fax);

            wasSent = true;
        }

        return wasSent;
    }
}
