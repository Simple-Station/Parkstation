using Robust.Shared.Configuration;

namespace Content.Shared.SimpleStation14.CCVar;

[CVarDefs]
public sealed class SimpleStationCCVars
{
    /// <summary>
    ///     Optionally force set an announcer
    /// </summary>
    public static readonly CVarDef<string> Announcer =
        CVarDef.Create("game.announcer", "random", CVar.SERVERONLY);

    /// <summary>
    ///     Optionally blacklist announcers
    /// </summary>
    public static readonly CVarDef<List<string>> AnnouncerBlacklist =
        CVarDef.Create("game.announcer.blacklist", new List<string>(), CVar.SERVERONLY);


    /// <summary>
    ///     Whether or not respawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RespawnEnabled =
        CVarDef.Create("respawn.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     How long the player has to wait in seconds after death before respawning.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("respawn.time", 600.0f, CVar.SERVER | CVar.REPLICATED);
}
