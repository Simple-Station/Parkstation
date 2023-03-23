using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.Cargo.Systems
{
    public sealed class CargoShuttleCostSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly CargoSystem _cargoSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        /// <summary>
        ///     An extra amount of money to add to the cost of the shuttle.
        /// </summary>
        public float GlobalDebt = 0f;
        /// <summary>
        ///     If we have attempted to call the shuttle (and failed)
        /// </summary>
        public bool AlreadyAttempted = false;

        public void CallShuttle(StationCargoOrderDatabaseComponent orderDatabase, EntityUid? player = null)
        {
            AlreadyAttempted = true;

            var cost = CalculateCost(orderDatabase);
            if (cost == null)
            {
                _cargoSystem.CallShuttle(orderDatabase);
                AlreadyAttempted = false;
                return;
            }

            var sm = SubtractMoney((float) cost);
            var success = sm.Item1;
            var debt = sm.Item2;

            if (success)
            {
                _cargoSystem.CallShuttle(orderDatabase);
                AlreadyAttempted = false;
            }
            else if (player != null && debt != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("cargo-shuttle-return-failed-explain", ("needed", debt)), player.Value, player.Value, PopupType.SmallCaution);
            }
            else if (player != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("cargo-shuttle-return-failed"), player.Value, player.Value, PopupType.SmallCaution);
            }

            // Add debt if we haven't tried already.
            if (debt != null && !AlreadyAttempted)
            {
                GlobalDebt += (float) debt;
            }
        }

        public void ReturnShuttle(StationCargoOrderDatabaseComponent orderDatabase, EntityUid? player = null)
        {
            if (orderDatabase.Shuttle == null) return;
            AlreadyAttempted = true;

            var cost = CalculateCost(orderDatabase);
            if (cost == null)
            {
                _cargoSystem.SendToCargoMap(orderDatabase.Shuttle.Value);
                AlreadyAttempted = false;
                return;
            }

            var sm = SubtractMoney((float) cost);
            var success = sm.Item1;
            var debt = sm.Item2;

            if (success)
            {
                _cargoSystem.SendToCargoMap(orderDatabase.Shuttle.Value);
                AlreadyAttempted = false;
            }
            else if (player != null && debt != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("cargo-shuttle-return-failed-explain", ("needed", debt)), player.Value, player.Value, PopupType.SmallCaution);
            }
            else if (player != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("cargo-shuttle-return-failed"), player.Value, player.Value, PopupType.SmallCaution);
            }

            // Add debt if we haven't tried already.
            if (debt != null && !AlreadyAttempted)
            {
                GlobalDebt += (float) debt;
            }
        }


        public float? CalculateCost(StationCargoOrderDatabaseComponent orderDatabase)
        {
            float? cost = null;

            var method = _configManager.GetCVar(SimpleStationCVars.CargoCostMethod);
            var mincost = _configManager.GetCVar(SimpleStationCVars.CargoShuttleBaseCost);
            var maxcost = _configManager.GetCVar(SimpleStationCVars.CargoShuttleMaxCost);

            // No methods, no cost.
            if (SimpleStationCVars.CargoShuttleCostMethods.Count == 0) return cost;
            // Check if method is valid, if not do the first possible method.
            if (!SimpleStationCVars.CargoShuttleCostMethods.Contains(method))
                method = SimpleStationCVars.CargoShuttleCostMethods.First();

            var bankAccount = _cargoSystem.GetBankAccount(EntityQuery<CargoOrderConsoleComponent>().First());
            if (bankAccount == null)
            {
                DebugTools.Assert("No bank account found for cargo shuttle.");
                return cost;
            }

            switch (method)
            {
                case "none":
                    break;
                case "flat":
                    cost = mincost;
                    break;
                case "timed":
                    var timedCost = _configManager.GetCVar(SimpleStationCVars.CargoShuttleTimedCost);
                    var time = _gameTiming.CurTime.TotalMinutes;
                    cost = (float?) (mincost + (timedCost * time));
                    break;
                case "percent":
                    var percentCost = _configManager.GetCVar(SimpleStationCVars.CargoShuttlePercentCost);
                    cost = (float?) (bankAccount.Balance * (percentCost / 100f));
                    break;
                case "load":
                    var loadCost = _configManager.GetCVar(SimpleStationCVars.CargoShuttleLoadCost);
                    var load = orderDatabase.Orders.Count;
                    cost = (float?) (mincost + (loadCost * load));
                    break;
                default:
                    DebugTools.Assert($"Unknown cargo shuttle cost method: {method}   Did you forget to add functionality?");
                    break;
            }

            // Clamp cost
            if (cost > maxcost) cost = maxcost;
            // Add debt to cost
            cost += GlobalDebt;

            return cost;
        }

        private (bool, float?) SubtractMoney(float cost)
        {
            var bankAccount = _cargoSystem.GetBankAccount(EntityQuery<CargoOrderConsoleComponent>().First());
            if (bankAccount == null)
            {
                DebugTools.Assert("No bank account found for cargo shuttle.");

                // Fail, no debt
                return (false, null);
            }

            if (bankAccount.Balance >= cost)
            {
                bankAccount.Balance -= (int) cost;
                GlobalDebt = 0f;

                // Success, no debt
                return (true, null);
            }
            else if (_configManager.GetCVar(SimpleStationCVars.CargoDebt))
            {
                // Success, debt
                return (true, cost > 0 ? cost : null);
            }

            // Fail, debt if allowed
            return (false, _configManager.GetCVar(SimpleStationCVars.CargoDebt) ? cost : null);
        }
    }
}
