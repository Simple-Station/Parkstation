using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Jukebox;
using Content.Shared.SimpleStation14.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Jukebox;

public sealed partial class JukeboxSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JukeboxComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<JukeboxComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<JukeboxComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<JukeboxComponent, JukeboxPlayButtonPressedMessage>(OnPlayButtonPressed);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSkipButtonPressedMessage>(OnSkipButtonPressed);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSongSelectedMessage>(OnSongSelected);
    }

    #region Event handlers

    /// <summary>
    ///     Simply checks if the Jukebox can play songs on init.
    /// </summary>
    private void OnComponentInit(EntityUid uid, JukeboxComponent component, ComponentInit args)
    {
        CheckCanPlay(uid, component);

        component.SerialNumber = GenerateSerialNumber();

        UpdateState(uid, component);
    }

    /// <summary>
    ///     Handles cleanup when the Jukebox is shut down.
    /// </summary>
    private void OnComponentShutdown(EntityUid uid, JukeboxComponent component, ComponentShutdown args)
    {
        Clean(uid, component);
    }

    #endregion Event handlers
    #region Public functions

    /// <summary>
    ///     Tries to play a song in the Jukebox using the provided <see cref="JukeboxTrackPrototype"/> ID.
    ///     If the Jukebox is already playing a song, the song will be added to the queue.
    /// </summary>
    /// <param name="song">The song to be queued.</param>
    public void TryQueueSong(EntityUid jukeBox, JukeboxComponent jukeboxComp, string song)
    {
        if (!jukeboxComp.CanPlay)
            return;

        if (jukeboxComp.CurrentlyPlayingTrack != null)
        {
            if (jukeboxComp.NextUp.Count < jukeboxComp.MaxQueued)
            {
                jukeboxComp.NextUp.Add(song);
            }

            UpdateState(jukeBox, jukeboxComp);

            return;
        }

        Play(jukeBox, jukeboxComp, song);
    }

    /// <summary>
    ///     Ends the currently playing song in the Jukebox and plays the next song in the queue, if available.
    /// </summary>
    public void Skip(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (!jukeboxComp.CanPlay)
            return;

        End(jukeBox, jukeboxComp);

        if (jukeboxComp.NextUp.Count > 0)
        {
            var toPlay = jukeboxComp.NextUp[0]; // Get the first song in the queue.
            jukeboxComp.NextUp.RemoveAt(0); // And remove it now so we don't need to UpdateState() twice.

            Play(jukeBox, jukeboxComp, toPlay);

            return;
        }

        UpdateState(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Pauses the currently playing song and sets the Jukebox to a paused state.
    /// </summary>
    /// <remarks>
    ///     See <see cref="Stop"/> to stop the Jukebox instead.
    ///     Pausing a Jukebox will allow it to remember its paused state, even if it gets stopped later.
    /// </remarks>
    public void Pause(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (jukeboxComp.Paused || !jukeboxComp.Playing)
            return;

        jukeboxComp.Paused = true;

        Stop(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Unpauses the Jukebox and resumes playing the current song from where it was paused.
    /// </summary>
    public void UnPause(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (!jukeboxComp.CanPlay)
            return;

        if (!jukeboxComp.Paused)
            return;

        jukeboxComp.Paused = false;

        TryRestart(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Plays a song in the Jukebox using the provided <see cref="JukeboxTrackPrototype"/> and optional offset.
    ///     If the Jukebox is already playing a song, it will be stopped and replaced with the new song.
    ///     To queue a song instead, see <see cref="TryQueueSong"/>.
    /// </summary>
    /// <param name="song">The JukeboxTrackPrototype representing the song to be played.</param>
    /// <param name="offset">The optional offset from the start of the song to begin playing from.</param>
    public void Play(EntityUid jukeBox, JukeboxComponent jukeboxComp, string song, TimeSpan offset = new TimeSpan())
    {
        if (!_prototype.TryIndex<JukeboxTrackPrototype>(song, out var songPrototype))
        {
            Logger.Error($"Jukebox track prototype {song} not found!");

            Skip(jukeBox, jukeboxComp);
            return;
        }

        Play(jukeBox, jukeboxComp, songPrototype, offset);
    }

    /// <inheritdoc cref="Play(EntityUid, JukeboxComponent, string, TimeSpan)"/>
    /// <remarks>
    ///     Directly takes a <see cref="JukeboxTrackPrototype"/> instead.
    /// </remarks>
    public void Play(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxTrackPrototype song, TimeSpan offset = new TimeSpan())
    {
        if (!jukeboxComp.CanPlay)
            return;

        Clean(jukeBox, jukeboxComp);

        jukeboxComp.Paused = false;

        jukeboxComp.Playing = true;

        jukeboxComp.CurrentlyPlayingTrack = song;

        jukeboxComp.FinishPlayingTime = _timing.CurTime + song.Duration - offset;

        jukeboxComp.CurrentlyPlayingStream = _audio.Play(song.Path, Filter.Broadcast(), jukeBox, true, AudioParams.Default.WithPlayOffset((float)offset.TotalSeconds));

        Timer.Spawn((int) (song.Duration.TotalMilliseconds - offset.TotalMilliseconds), () => OnSongEnd(jukeBox, jukeboxComp), jukeboxComp.SongTimerCancel.Token);

        UpdateState(jukeBox, jukeboxComp);
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
    ///     See <see cref="Pause"/> to pause the Jukebox instead.
    /// </remarks>
    private void Stop(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        if (!jukeboxComp.Playing)
            return;

        jukeboxComp.Playing = false;

        jukeboxComp.StoppedTime = _timing.CurTime;

        if (jukeboxComp.CurrentlyPlayingStream != null)
        {
            jukeboxComp.CurrentlyPlayingStream.Stop();
            jukeboxComp.CurrentlyPlayingStream = null;
        }

        jukeboxComp.SongTimerCancel.Cancel();

        UpdateState(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Ends the currently playing song in the Jukebox.
    /// </summary>
    private void End(EntityUid jukeBox, JukeboxComponent jukeboxComp)
    {
        jukeboxComp.CurrentlyPlayingTrack = null;

        jukeboxComp.FinishPlayingTime = null;

        jukeboxComp.StoppedTime = null;

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

        jukeboxComp.SongTimerCancel.Cancel();

        UpdateState(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Tries to unpause the Jukebox and continue playing the current song from where it was paused.
    /// </summary>
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

        Play(jukeBox, jukeboxComp, jukeboxComp.CurrentlyPlayingTrack, timeLeftBeforeFinished);

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
        Skip(jukeBox, jukeboxComp);
    }

    #endregion Private functions
}
