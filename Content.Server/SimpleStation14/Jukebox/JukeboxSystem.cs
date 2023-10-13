using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Jukebox;
using Content.Shared.SimpleStation14.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Content.Shared.Emag.Systems;
using Content.Server.Construction;
using Content.Shared.Damage;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Jukebox;

public sealed partial class JukeboxSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DeviceLinkSystem _link = default!;

    public const string PortSongPlayed = "Start";
    public const string PortSongStopped = "Timer";

    public const string PortPlayRandom = "PlayRandom";
    public const string PortSkip = "Skip";
    public const string PortPause = "Pause";
    public const string PortUnPause = "Unpause";
    public const string PortTogglePuase = "TogglePause";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JukeboxComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<JukeboxComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<JukeboxComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<JukeboxComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<JukeboxComponent, EntityPausedEvent>(OnPaused);
        SubscribeLocalEvent<JukeboxComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<JukeboxComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<JukeboxComponent, UpgradeExamineEvent>(OnExamineParts);
        SubscribeLocalEvent<JukeboxComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<JukeboxComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<JukeboxComponent, JukeboxPlayButtonPressedMessage>(OnPlayButtonPressed);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSkipButtonPressedMessage>(OnSkipButtonPressed);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSongSelectedMessage>(OnSongSelected);
    }

    #region Public functions

    /// <summary>
    ///     Tries to play a song in the Jukebox using the provided <see cref="JukeboxTrackPrototype"/> ID.
    ///     If the Jukebox is already playing a song, the song will be added to the queue.
    /// </summary>
    /// <param name="song">The song to be queued.</param>
    public void TryQueueSong(EntityUid jukebox, string song, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (!jukeboxComp.CanPlay)
            return;

        if (jukeboxComp.CurrentlyPlayingTrack != null)
        {
            if (jukeboxComp.NextUp.Count < jukeboxComp.MaxQueued)
            {
                jukeboxComp.NextUp.Add(song);
            }

            UpdateState(jukebox, jukeboxComp);

            return;
        }

        TryPlaySong(jukebox, song, jukeboxComp);
    }

    /// <summary>
    ///     Ends the currently playing song in the Jukebox and plays the next song in the queue, if available.
    /// </summary>
    public void TrySkipSong(EntityUid jukebox, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (!jukeboxComp.CanPlay)
            return;

        End(jukebox, jukeboxComp);

        if (jukeboxComp.NextUp.Count > 0)
        {
            var toPlay = jukeboxComp.NextUp[0]; // Get the first song in the queue.
            jukeboxComp.NextUp.RemoveAt(0); // And remove it now so we don't need to UpdateState() twice.

            TryPlaySong(jukebox, toPlay, jukeboxComp);

            return;
        }

        UpdateState(jukebox, jukeboxComp);
    }

    /// <summary>
    ///     Pauses the currently playing song and sets the Jukebox to a paused state.
    /// </summary>
    /// <remarks>
    ///     See <see cref="Stop"/> to stop the Jukebox instead.
    ///     Pausing a Jukebox will allow it to remember its paused state, even if it gets stopped later.
    /// </remarks>
    public void DoPauseSong(EntityUid jukebox, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (jukeboxComp.Paused || !jukeboxComp.Playing)
            return;

        jukeboxComp.Paused = true;

        Stop(jukebox, jukeboxComp);
    }

    /// <summary>
    ///     Unpauses the Jukebox and resumes playing the current song from where it was paused.
    /// </summary>
    public void TryUnPauseSong(EntityUid jukebox, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (!jukeboxComp.CanPlay)
            return;

        if (!jukeboxComp.Paused)
            return;

        jukeboxComp.Paused = false;

        TryRestart(jukebox, jukeboxComp);
    }

    /// <summary>
    ///    Toggles the Jukebox's paused state.
    /// </summary>
    public void TryTogglePause(EntityUid jukebox, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (jukeboxComp.Paused)
            TryUnPauseSong(jukebox, jukeboxComp);
        else
            DoPauseSong(jukebox, jukeboxComp);
    }

    public void TryPlayRandomSong(EntityUid jukebox, JukeboxComponent? jukeboxComp = null)
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        var potentialSongs = jukeboxComp.Songs;

        if (jukeboxComp.Emagged)
            potentialSongs.AddRange(jukeboxComp.EmaggedSongs);

        TryPlaySong(jukebox, _random.Pick(potentialSongs), jukeboxComp);
    }

    /// <summary>
    ///     Plays a song in the Jukebox using the provided <see cref="JukeboxTrackPrototype"/> and optional offset.
    ///     If the Jukebox is already playing a song, it will be stopped and replaced with the new song.
    ///     To queue a song instead, see <see cref="TryQueueSong"/>.
    /// </summary>
    /// <param name="song">The JukeboxTrackPrototype representing the song to be played.</param>
    /// <param name="offset">The optional offset from the start of the song to begin playing from.</param>
    public void TryPlaySong(EntityUid jukebox, string song, JukeboxComponent? jukeboxComp = null, TimeSpan offset = new TimeSpan())
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (!_prototype.TryIndex<JukeboxTrackPrototype>(song, out var songPrototype))
        {
            Logger.Error($"Jukebox track prototype {song} not found!");
            return;
        }

        TryPlaySong(jukebox, songPrototype, jukeboxComp, offset);
    }

    /// <inheritdoc cref="TryPlaySong(EntityUid, JukeboxComponent, string, TimeSpan)"/>
    /// <remarks>
    ///     Directly takes a <see cref="JukeboxTrackPrototype"/> instead.
    /// </remarks>
    public void TryPlaySong(EntityUid jukebox, JukeboxTrackPrototype song, JukeboxComponent? jukeboxComp = null, TimeSpan offset = new TimeSpan())
    {
        if (!Resolve(jukebox, ref jukeboxComp))
            return;

        if (!jukeboxComp.CanPlay)
            return;

        if (offset > song.Duration) // Just to make sure we don't try to play a song from the future.
            offset = song.Duration - TimeSpan.FromSeconds(0.1);

        Clean(jukebox, jukeboxComp); // Clean up any currently playing song.

        jukeboxComp.Paused = false; // Unpause the Jukebox if it was paused.

        jukeboxComp.Playing = true; // Set the Jukebox to playing.

        jukeboxComp.CurrentlyPlayingTrack = song; // Set the currently playing song.

        jukeboxComp.FinishPlayingTime = _timing.CurTime + song.Duration - offset; // Set the time when the song should finish.

        jukeboxComp.CurrentlyPlayingStream = _audio.Play(song.Path, Filter.Broadcast(), jukebox, true, AudioParams.Default.WithPlayOffset((float) offset.TotalSeconds)); // Play the song, with any offset, and to every player in the game.

        _link.InvokePort(jukebox, PortSongPlayed); // Invoke the Jukebox's "Start" port.

        UpdateState(jukebox, jukeboxComp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<JukeboxComponent>();

        // Iterate over all Jukeboxes.
        while (query.MoveNext(out var jukebox, out var jukeboxComp))
        {
            // If the Jukebox is playing, and the song has finished, end the song.
            // This was orignially done by starting a timer, but that stopped working out of nowhere, so...
            if (jukeboxComp.Playing && _timing.CurTime >= jukeboxComp.FinishPlayingTime)
            {
                OnSongEnd(jukebox, jukeboxComp);
            }
        }
    }

    #endregion Public functions
    #region Private functions

    /// <summary>
    ///     Sets the Jukebox to either a state where it can or cannot play songs.
    /// </summary>
    private void CheckCanPlay(EntityUid uid, JukeboxComponent component)
    {
        var canPlay = true;

        if (EntityManager.TryGetComponent<ApcPowerReceiverComponent>(uid, out var powerComp) && !powerComp.Powered)
            canPlay = false;

        component.CanPlay = canPlay;

        if (canPlay)
            TryRestart(uid, component);
        else
            Stop(uid, component);
    }

    /// <summary>
    ///     Stops the currently playing song and sets the Jukebox to a stopped state.
    /// </summary>
    /// <remarks>
    ///     See <see cref="DoPauseSong"/> to pause the Jukebox instead.
    /// </remarks>
    private void Stop(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (!jukeboxComp.Playing)
            return;

        jukeboxComp.Playing = false;

        jukeboxComp.StoppedTime = _timing.CurTime;

        Clean(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Ends the currently playing song in the Jukebox.
    /// </summary>
    private void End(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        jukeboxComp.CurrentlyPlayingTrack = null;

        jukeboxComp.FinishPlayingTime = null;

        jukeboxComp.StoppedTime = null;

        _link.InvokePort(jukeBox, PortSongStopped); // Invoke the Jukebox's "Timer" port.

        Clean(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Cleans up the active elements of a playing song.
    /// </summary>
    private void Clean(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (jukeboxComp.CurrentlyPlayingStream != null)
        {
            jukeboxComp.CurrentlyPlayingStream.Stop();
            jukeboxComp.CurrentlyPlayingStream = null;
        }

        UpdateState(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Tries to restart the Jukebox and continue playing the current song from where it was stopped.
    /// </summary>
    /// <remarks>
    ///     Note, this will not unpause the Jukebox itself. The Jukebox must first be unpaused, and then restarted.
    /// </remarks>
    /// <returns>Whether or not the Jukebox is now playing.</returns>
    private void TryRestart(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (jukeboxComp.Paused || !jukeboxComp.CanPlay)
            return;

        jukeboxComp.Playing = true;

        if (jukeboxComp.CurrentlyPlayingTrack == null || jukeboxComp.FinishPlayingTime == null || jukeboxComp.StoppedTime == null)
        {
            UpdateState(jukeBox, jukeboxComp);
            return;
        }

        var timeLeftBeforeFinished = (TimeSpan) (jukeboxComp.CurrentlyPlayingTrack.Duration - (jukeboxComp.FinishPlayingTime - jukeboxComp.StoppedTime));

        jukeboxComp.StoppedTime = null;

        TryPlaySong(jukeBox, jukeboxComp.CurrentlyPlayingTrack, jukeboxComp, timeLeftBeforeFinished);

        return;
    }

    /// <summary>
    ///     Updates the Jukebox's state and ui.
    /// </summary>
    /// <param name="populateSongs">Whether or not to populate the song list in the ui.</param>
    private void UpdateState(EntityUid jukeBox, JukeboxComponent jukeboxComp, bool populateSongs = false)
    {
        Dirty(jukeboxComp);

        _ui.TrySendUiMessage(jukeBox, JukeboxUiKey.Key, new JukeboxUpdateStateMessage(populateSongs));
    }

    /// <summary>
    ///     Event handler for the song in the Jukebox reaching its end.
    /// </summary>
    private void OnSongEnd(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        TrySkipSong(jukeBox, jukeboxComp);
    }

    #endregion Private functions
}
