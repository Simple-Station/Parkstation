using System.Data;
using System.Linq;
using Content.Server.Fax;
using Content.Shared.GameTicking;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Corvax.StationGoal
{
    /// <summary>
    ///     System to spawn paper with station goal.
    /// </summary>
    public sealed class StationGoalPaperSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly FaxSystem _faxSystem = default!;

        private const string RandomPrototype = "StationGoals";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        }

        private void OnRoundStarted(RoundStartedEvent ev)
        {
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
            var faxes = EntityManager.EntityQuery<FaxMachineComponent>();
            var wasSent = false;

            foreach (var fax in faxes.Where(f => f.ReceiveStationGoal))
            {
                var printout = new FaxPrintout(
                    Loc.GetString(goal.Text),
                    Loc.GetString("station-goal-fax-paper-name"),
                    null,
                    "paper_stamp-cent",
                    new List<string> { Loc.GetString("stamp-component-stamped-name-centcom") }
                );

                _faxSystem.Receive(fax.Owner, printout, null, fax);

                wasSent = true;
            }

            return wasSent;
        }
    }
}
