using Content.Server.Construction;
using Content.Server.DeviceLinking.Events;
using Content.Server.Power.Components;
using Content.Shared.Damage;
using Content.Shared.Emag.Systems;
using Content.Shared.SimpleStation14.Jukebox;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Jukebox;

public sealed partial class JukeboxSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    ///     Simply checks if the Jukebox can play songs on init.
    /// </summary>
    private void OnComponentInit(EntityUid uid, JukeboxComponent component, ComponentInit args)
    {
        CheckCanPlay(uid, component);

        component.SerialNumber = GenerateSerialNumber();

        _link.EnsureSourcePorts(uid, PortSongPlayed);
        _link.EnsureSourcePorts(uid, PortSongStopped);

        _link.EnsureSinkPorts(uid, PortPlayRandom);
        _link.EnsureSinkPorts(uid, PortSkip);
        _link.EnsureSinkPorts(uid, PortPause);
        _link.EnsureSinkPorts(uid, PortUnPause);
        _link.EnsureSinkPorts(uid, PortTogglePuase);

        UpdateState(uid, component);
    }

    /// <summary>
    ///     Handles cleanup when the Jukebox is shut down.
    /// </summary>
    private void OnComponentShutdown(EntityUid uid, JukeboxComponent component, ComponentShutdown args)
    {
        Clean(uid, component);
    }

    /// <summary>
    ///     Handles setting the Jukebox's state to emagged.
    /// </summary>
    private void OnEmagged(EntityUid jukeBox, JukeboxComponent jukeboxComp, ref GotEmaggedEvent args)
    {
        jukeboxComp.Emagged = true;
        args.Handled = true;
        UpdateState(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///    Handles when a Jukebox entity is paused in game terms.
    /// </summary>
    private void OnPaused(EntityUid jukeBox, JukeboxComponent jukeboxComp, ref EntityPausedEvent args)
    {
        StopSong(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///   Handles when a Jukebox entity is unpaused in game terms.
    /// </summary>
    private void OnUnpaused(EntityUid jukeBox, JukeboxComponent jukeboxComp, ref EntityUnpausedEvent args)
    {
        CheckCanPlay(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Checks if the Jukebox can play songs when its power state changes.
    /// </summary>
    private void OnPowerChanged(EntityUid uid, JukeboxComponent component, ref PowerChangedEvent args)
    {
        CheckCanPlay(uid, component);
    }

    /// <summary>
    ///     Handles the Jukebox's play button being pressed to toggle between playing and paused.
    /// </summary>
    private void OnPlayButtonPressed(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxPlayButtonPressedMessage msg)
    {
        if (jukeboxComp.Paused)
        {
            TryUnPauseSong(jukeBox, jukeboxComp);
        }
        else
        {
            DoPauseSong(jukeBox, jukeboxComp);
        }
    }

    /// <summary>
    ///     Handles the Jukebox's skip button being pressed to skip the current song.
    /// </summary>
    private void OnSkipButtonPressed(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxSkipButtonPressedMessage msg)
    {
        TrySkipSong(jukeBox, jukeboxComp);
    }

    /// <summary>
    ///     Handles a song being selected in the Jukebox's ui.
    /// </summary>
    private void OnSongSelected(EntityUid jukeBox, JukeboxComponent jukeboxComp, JukeboxSongSelectedMessage msg)
    {
        TryQueueSong(jukeBox, msg.Song, jukeboxComp);
    }

    /// <summary>
    ///     Generates a valid serial number for the jukeboxes. This is just goofy.
    /// </summary>
    public string GenerateSerialNumber()
    {
        var digits = new int[8];

        digits[0] = _random.Next(1, 10);
        digits[1] = _random.Next(10);
        digits[2] = digits[1];
        digits[3] = digits[0];
        digits[4] = (digits[0] + digits[1]) % 10;
        digits[5] = digits[2] - digits[3];
        digits[6] = digits[4] * digits[5];
        digits[7] = (digits[0] + digits[1] + digits[2] + digits[3]) % 5;

        var serial = $"{digits[0]}{digits[1]}{digits[2]}{digits[3]}{digits[4]}{digits[5]}{digits[6]}";

        var letter = digits[7] == 0 ? 90 : digits[7] % 2 == 0 ? (digits[7] + 65) : (90 - digits[7]);

        return $"{serial}{(char) letter}";
    }

    private void OnRefreshParts(EntityUid uid, JukeboxComponent component, RefreshPartsEvent args)
    {
        if (component.QueueSizeUpgradePart == null)
        {
            component.MaxQueued = component.MaxQueuedDefault;
            return;
        }

        var queueSizeMod = (int) Math.Floor(args.PartRatings[component.QueueSizeUpgradePart]);

        component.MaxQueued = component.MaxQueuedDefault * queueSizeMod;
    }

    private void OnExamineParts(EntityUid uid, JukeboxComponent component, UpgradeExamineEvent args)
    {
        if (component.QueueSizeUpgradePart == null)
        {
            return;
        }

        args.AddNumberUpgrade("jukebox-maxqueued-upgrade-string", component.MaxQueued - component.MaxQueuedDefault);
    }

    private void OnDamageChanged(EntityUid uid, JukeboxComponent component, DamageChangedEvent args)
    {
        if (args.DamageIncreased && args.DamageDelta != null && args.DamageDelta.Total < 7 && _random.Prob(0.65f))
            TryPlayRandomSong(uid, component);
    }

    private void OnSignalReceived(EntityUid uid, JukeboxComponent component, ref SignalReceivedEvent args)
    {
        switch (args.Port)
        {
            case PortPlayRandom:
                TryPlayRandomSong(uid, component);
                break;
            case PortSkip:
                TrySkipSong(uid, component);
                break;
            case PortPause:
                DoPauseSong(uid, component);
                break;
            case PortUnPause:
                TryUnPauseSong(uid, component);
                break;
            case PortTogglePuase:
                TryTogglePause(uid, component);
                break;
            default:
                return;
        }
    }
}
