using System.Collections.Generic;
using Content.Shared.SimpleStation14.Prototypes;
using Robust.Shared.Audio;
using System.Threading;
using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Jukebox;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
public sealed partial class JukeboxComponent : Component
{
    /// <summary>
    ///     The list of songs that the Jukebox has in its playlist.
    /// </summary>
    [AutoNetworkedField]
    [DataField("songs")] [ViewVariables(VVAccess.ReadWrite)]
    public List<string> Songs { get; set; } = new();

    /// <summary>
    ///     The list of songs that the Jukebox gains access to when emagged.
    /// </summary>
    [AutoNetworkedField]
    [DataField("emaggedSongs")] [ViewVariables(VVAccess.ReadWrite)]
    public List<string> EmaggedSongs { get; set; } = new();

    /// <summary>
    ///     The maximum number of songs that can be queued at one time.
    /// </summary>
    [DataField("maxQueued")]
    public int MaxQueued { get; set; } = 3;

    /// <summary>
    ///     The song art to be used when no song is playing.
    /// </summary>
    [AutoNetworkedField]
    [DataField("defaultSongArtPath")]
    public string DefaultSongArtPath { get; set; } = "/Textures/SimpleStation14/JukeboxTracks/default.png";

    /// <summary>
    ///     A colour to be used in the Jukebox's UI.
    ///     Should be based on the sprite.
    /// </summary>
    [DataField("jukeboxBG")]
    public string JukeboxBG { get; set; } = "#602C00";

    /// <inheritdoc cref="JukeboxBG"/>
    [DataField("jukeboxPanel")]
    public string JukeboxPanel { get; } = "#480F0F";

    /// <inheritdoc cref="JukeboxBG"/>
    [DataField("jukeboxAccent")]
    public string JukeboxAccent { get; } = "#20181B";

    /// <summary>
    ///     The currently playing audio stream.
    /// </summary>
    public IPlayingAudioStream? CurrentlyPlayingStream { get; set; }

    /// <summary>
    ///     The cancellation token source for the song timer.
    /// </summary>

    public CancellationTokenSource SongTimerCancel { get; set; } = new();

    /// <summary>
    ///     The prototype of the currently playing song.
    /// </summary>
    [AutoNetworkedField]
    public JukeboxTrackPrototype? CurrentlyPlayingTrack { get; set; }

    /// <summary>
    ///     The list of songs that are queued to be played.
    /// </summary>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadWrite)]
    public List<string> NextUp { get; set; } = new();

    /// <summary>
    ///     The time when the currently playing song should finish.
    /// </summary>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? FinishPlayingTime { get; set; } = null;

    /// <summary>
    ///     The time when the currently playing song was stopped.
    /// </summary>
    /// <remarks>
    ///     Used to calculate the time remaining when a song is paused, or stopped by other means.
    /// </remarks>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? StoppedTime { get; set; } = null;

    /// <summary>
    ///     Whether or not the Jukebox should currently be playing.
    /// </summary>
    /// <remarks>
    ///     This can still be true if no song is playing. It's essentially <see cref="Paused"/> but not controlled by the user.
    /// </remarks>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadOnly)]
    public bool Playing { get; set; } = true;

    /// <summary>
    ///     Whether or not the Jukebox is currently paused.
    /// </summary>
    /// <remarks>
    ///     The Jukebox may be unpaused, but not playing if <see cref="Playing"/> is false.
    /// </remarks>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadOnly)]
    public bool Paused { get; set; } = false;

    /// <summary>
    ///     Whether or not the Jukebox is actually active.
    /// </summary>
    /// <remarks>
    ///     Generally controlled server-side, for things such as power, where a function isn't appropriate.
    ///     To trigger a change in this value, use <see cref="SharedJukeboxSystem.SetCanPlay"/>.
    /// </remarks>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadOnly)]
    public bool CanPlay { get; set; } = true;

    /// <summary>
    ///     If the Jukebox has been emagged.
    /// </summary>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadWrite)]
    public bool Emagged { get; set; } = false;

    /// <summary>
    ///     A serial number, following convuluted rules. Just for fun :)
    /// </summary>
    [AutoNetworkedField] [ViewVariables(VVAccess.ReadWrite)]
    public string SerialNumber { get; set; } = "000000000";
}
