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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JukeboxComponent, MapInitEvent>(OnComponentInit);
        SubscribeLocalEvent<JukeboxComponent, ComponentShutdown>((ent, comp, _) => Clean(ent, comp));

        SubscribeLocalEvent((EntityUid ent, JukeboxComponent comp, ref EntityPausedEvent _) => CheckCanPlay(ent, comp));
        SubscribeLocalEvent((EntityUid ent, JukeboxComponent comp, ref EntityUnpausedEvent _) => CheckCanPlay(ent, comp));
        SubscribeLocalEvent((EntityUid ent, JukeboxComponent comp, ref PowerChangedEvent _) => CheckCanPlay(ent, comp));
        SubscribeLocalEvent<JukeboxComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<JukeboxComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<JukeboxComponent, UpgradeExamineEvent>(OnExamineParts);
        SubscribeLocalEvent<JukeboxComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<JukeboxComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<JukeboxComponent, JukeboxPlayButtonPressedMessage>((ent, comp, _) => TryTogglePause(ent, comp));
        SubscribeLocalEvent<JukeboxComponent, JukeboxSkipButtonPressedMessage>((ent, comp, _) => TrySkipSong(ent, comp));
        SubscribeLocalEvent<JukeboxComponent, JukeboxSongSelectedMessage>((ent, comp, msg) => TryQueueSong(ent, msg.Song, comp));
    }

    /// <summary>
    ///     Handles setting up a Jukebox.
    /// </summary>
    private void OnComponentInit(EntityUid uid, JukeboxComponent component, MapInitEvent args)
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
    ///     Handles setting the Jukebox's state to emagged.
    /// </summary>
    private void OnEmagged(EntityUid jukeBox, JukeboxComponent jukeboxComp, ref GotEmaggedEvent args)
    {
        jukeboxComp.Emagged = true;
        args.Handled = true;
        UpdateState(jukeBox, jukeboxComp);
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
        digits[7] = (digits[0] + digits[1] + digits[5] + digits[4]) % 5;

        var letter = digits[7] == 0 ? 90 : digits[7] % 2 == 0 ? (digits[7] + 65) : (90 - digits[7]);

        var serial = $"{digits[0]}{digits[1]}{digits[2]}{digits[3]}{digits[4]}{digits[Math.Abs(5)]}{digits[Math.Abs(6)]}{digits[7]}{(char) letter}";

        return serial;
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
