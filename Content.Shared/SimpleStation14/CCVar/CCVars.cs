using Robust.Shared.Configuration;

namespace Content.Shared.SimpleStation14.CCVar;

[CVarDefs]
public sealed class SimpleStationCVars
{
    /// <summary>
    ///     Optionally force set an announcer
    /// </summary>
    public static readonly CVarDef<string> Announcer =
        CVarDef.Create("game.announcer", "random", CVar.SERVERONLY);
}
