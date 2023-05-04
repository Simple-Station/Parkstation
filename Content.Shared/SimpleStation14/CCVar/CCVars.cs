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

    /*
     * End of round stats
     */

    #region BloodLost
    /// <summary>
    ///     The amount of blood lost required to trigger the BloodLost end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the BloodLost end of round stat.
    /// </remarks>
    public static readonly CVarDef<float> BloodLostThreshold =
        CVarDef.Create("eorstats.bloodlost_threshold", 300f, CVar.SERVERONLY);
    #endregion

    #region CuffedTime
    /// <summary>
    ///     The amount of time required to trigger the CuffedTime end of round stat, in minutes.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the CuffedTime end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> CuffedTimeThreshold =
        CVarDef.Create("eorstats.cuffedtime_threshold", 8, CVar.SERVERONLY);
    #endregion

    #region EmitSound
    /// <summary>
    ///     The amount of sounds required to trigger the EmitSound end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the EmitSound end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> EmitSoundThreshold =
        CVarDef.Create("eorstats.emitsound_threshold", 80, CVar.SERVERONLY);
    #endregion

    #region InstrumentPlayed
    /// <summary>
    ///     The amount of instruments required to trigger the InstrumentPlayed end of round stat, in minutes.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the InstrumentPlayed end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> InstrumentPlayedThreshold =
        CVarDef.Create("eorstats.instrumentplayed_threshold", 8, CVar.SERVERONLY);
    #endregion

    #region MopUsed
    /// <summary>
    ///     The amount of liquid mopped required to trigger the MopUsed end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the MopUsed end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> MopUsedThreshold =
        CVarDef.Create("eorstats.mopused_threshold", 200, CVar.SERVERONLY);

    /// <summary>
    ///     Should a stat be displayed specifically when no mopping was done?
    /// </summary>
    public static readonly CVarDef<bool> MopUsedDisplayNone =
        CVarDef.Create("eorstats.mopused_displaynone", true, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of top moppers to show in the end of round stats.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the top moppers.
    /// </remarks>
    public static readonly CVarDef<int> MopUsedTopMopperCount =
        CVarDef.Create("eorstats.mopused_topmoppercount", 3, CVar.SERVERONLY);
    #endregion

    #region ShotsFired
    /// <summary>
    ///     The amount of shots fired required to trigger the ShotsFired end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the ShotsFired end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> ShotsFiredThreshold =
        CVarDef.Create("eorstats.shotsfired_threshold", 40, CVar.SERVERONLY);

    /// <summary>
    ///     Should a stat be displayed specifically when no shots were fired?
    /// </summary>
    public static readonly CVarDef<bool> ShotsFiredDisplayNone =
        CVarDef.Create("eorstats.shotsfired_displaynone", true, CVar.SERVERONLY);
    #endregion
}
