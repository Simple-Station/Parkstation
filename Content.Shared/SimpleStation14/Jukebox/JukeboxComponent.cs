using Content.Shared.SimpleStation14.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Construction.Prototypes;
using Content.Shared.DeviceLinking;

namespace Content.Shared.SimpleStation14.Jukebox;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class JukeboxComponent : Component
{
    /// <summary>
    ///     The list of songs that the Jukebox has in its playlist.
    /// </summary>
    [AutoNetworkedField]
    [DataField("songs")]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> Songs { get; set; } = new();

    /// <summary>
    ///     The list of songs that the Jukebox gains access to when emagged.
    /// </summary>
    [AutoNetworkedField]
    [DataField("emaggedSongs")]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> EmaggedSongs { get; set; } = new();

    /// <summary>
    ///     The maximum number of songs that can be queued at one time by default, with the lowest tier parts.
    /// </summary>
    [DataField("maxQueuedDefault")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxQueuedDefault { get; set; } = 3;

    /// <summary>
    ///    The maximum number of songs that can be queued at one time, with upgrades.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxQueued { get; set; }

    /// <summary>
    ///     The song art to be used when no song is playing.
    /// </summary>
    [DataField("defaultSongArtPath")]
    public string DefaultSongArtPath { get; } = "/Textures/SimpleStation14/JukeboxTracks/default.png";

    /// <summary>
    ///     A colour to be used in the Jukebox's UI.
    ///     Should be based on the sprite.
    /// </summary>
    [DataField("uiColorBG")]
    public string UiColorBG { get; } = "#602C00";

    /// <inheritdoc cref="UiColorBG"/>
    [DataField("uiColorPanel")]
    public string UiColorPanel { get; } = "#480F0F";

    /// <inheritdoc cref="UiColorBG"/>
    [DataField("uiColorAccent")]
    public string UiColorAccent { get; } = "#20181B";

    [DataField("uiButtonPlay")]
    public string UiButtonPlay { get; } = "/Textures/SimpleStation14/Interface/MediaControls/play.png";

    [DataField("uiButtonPause")]
    public string UiButtonPause { get; } = "/Textures/SimpleStation14/Interface/MediaControls/pause.png";

    [DataField("uiButtonSkip")]
    public string UiButtonSkip { get; } = "/Textures/SimpleStation14/Interface/MediaControls/skip.png";

    /// <summary>
    ///    Whether or not to include the decorative portion of the UI
    ///    which contains the serial number and the 'coin' slot.
    /// </summary>
    [DataField("decorativeUi")]
    public bool DecorativeUi { get; } = false;

    /// <summary>
    ///     The part to be used for upgrading the queue size.
    /// </summary>
    /// <remarks>
    ///     Leave empty to disable queue size upgrades.
    /// </remarks>
    [DataField("queueSizeUpgradePart", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? QueueSizeUpgradePart { get; set; }

    /// <summary>
    ///     The currently playing audio stream.
    /// </summary>
    public IPlayingAudioStream? CurrentlyPlayingStream { get; set; }

    /// <summary>
    ///     The ID of the currently playing song.
    /// </summary>
    [AutoNetworkedField]
    public string? CurrentlyPlayingTrack { get; set; }

    /// <summary>
    ///     The list of songs that are queued to be played.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> NextUp { get; set; } = new();

    /// <summary>
    ///     The time when the currently playing song should finish.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? FinishPlayingTime { get; set; } = null;

    /// <summary>
    ///     The time when the currently playing song was stopped.
    /// </summary>
    /// <remarks>
    ///     Used to calculate the time remaining when a song is paused, or stopped by other means.
    /// </remarks>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? StoppedTime { get; set; } = null;

    /// <summary>
    ///     Whether or not the Jukebox should currently be playing.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Playing { get; set; } = false;

    /// <summary>
    ///     Whether or not the Jukebox is currently paused.
    /// </summary>
    /// <remarks>
    ///     The Jukebox may be unpaused, but not playing if <see cref="Playing"/> is false.
    /// </remarks>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Paused { get; set; } = false;

    /// <summary>
    ///     Whether or not the Jukebox is actually active.
    /// </summary>
    /// <remarks>
    ///     Generally controlled server-side, for things such as power, where a function isn't appropriate.
    ///     To trigger a change in this value, use <see cref="SharedJukeboxSystem.SetCanPlay"/>.
    /// </remarks>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanPlay { get; set; } = true;

    /// <summary>
    ///     If the Jukebox has been emagged.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Emagged { get; set; } = false;

    /// <summary>
    ///     A serial number, following convuluted rules. Just for fun :)
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string SerialNumber { get; set; } = "000000000";
}
