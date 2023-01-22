using Robust.Shared.Configuration;

namespace Content.Shared.SimpleStation14.CCVar;

[CVarDefs]
public sealed class SimpleStationCVars
{
    /// <summary>
    /// How much money to remove from cargo when the shuttle FTLs
    /// </summary>
    public static readonly CVarDef<int> CargoShuttleCost =
        CVarDef.Create("cargo.shuttle.cost", 100, CVar.SERVER);
}
