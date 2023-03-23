using Robust.Shared.Configuration;

namespace Content.Shared.SimpleStation14.CCVar;

[CVarDefs]
public sealed class SimpleStationCVars
{
    public static readonly List<string> CargoShuttleCostMethods = new() {"flat", "timed", "percent", "load", "none"};

    /// <summary>
    ///     The method used to calculate the cost of the cargo shuttle.
    /// </summary>
    /// <remarks>
    ///     Valid values are stored in <see cref="CargoShuttleCostMethods"/>.
    /// </remarks>
    public static readonly CVarDef<string> CargoCostMethod =
        CVarDef.Create("cargo.shuttle.method", "flat", CVar.SERVER);

    /// <summary>
    ///     Allow the cost to increase with debt but still call the shuttle if you can't pay it.
    /// </summary>
    public static readonly CVarDef<bool> CargoDebt =
        CVarDef.Create("cargo.debt", false, CVar.SERVER);

    /// <summary>
    ///     The minimum/starting amount for the cargo shuttle to use when flying.
    /// </summary>
    public static readonly CVarDef<int> CargoShuttleBaseCost =
        CVarDef.Create("cargo.shuttle.basecost", 100, CVar.SERVER);

    /// <summary>
    ///     The maximum amount of money the cargo shuttle will cost.
    /// </summary>
    public static readonly CVarDef<int> CargoShuttleMaxCost =
        CVarDef.Create("cargo.shuttle.maxcost", 1000, CVar.SERVER);

    /// <summary>
    ///     The amount of money the cargo shuttle will cost per minute.
    /// </summary>
    public static readonly CVarDef<int> CargoShuttleTimedCost =
        CVarDef.Create("cargo.shuttle.timedcost", 5, CVar.SERVER);

    /// <summary>
    ///     The percent of money to take when flying the cargo shuttle.
    /// </summary>
    public static readonly CVarDef<int> CargoShuttlePercentCost =
        CVarDef.Create("cargo.shuttle.percentcost", 10, CVar.SERVER);

    /// <summary>
    ///     The amount of money the cargo shuttle will cost per order.
    /// </summary>
    public static readonly CVarDef<int> CargoShuttleLoadCost =
        CVarDef.Create("cargo.shuttle.loadcost", 75, CVar.SERVER);
}
