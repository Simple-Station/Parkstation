using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.Pulling;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Player;
using Content.Shared.Pulling.Components;
using Linguini.Syntax.Ast;
using Robust.Shared.Timing;
using JetBrains.Annotations;
using Content.Shared.SimpleStation14.Holograms.Components;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class HologramSystem : SharedHologramSystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public readonly Dictionary<Mind.Mind, EntityUid> ClonesWaitingForMind = new();

    public override void Initialize()
    {
        base.Initialize();
        // SubscribeLocalEvent<HologramComponent, ComponentStartup>(Startup);
        // SubscribeLocalEvent<HologramComponent, ComponentShutdown>(Shutdown);
        // SubscribeLocalEvent<HoloTeleportEvent>(HoloTeleport);
    }

    // private void Startup(EntityUid uid, HologramComponent component, ComponentStartup args)
    // {
    //     var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
    //     _actionsSystem.AddAction(uid, action, uid);
    // }

    // private void Shutdown(EntityUid uid, HologramComponent component, ComponentShutdown args)
    // {
    //     var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
    //     _actionsSystem.RemoveAction(uid, action);
    // }

    /// <summary>
    ///     Mind stuff is all server side, so this exists to actually kill the Hologram in reaction to <see cref="SharedHologramSystem.DoReturnHologram(EntityUid, out bool)"/>.
    /// </summary>
    public override void DoReturnHologram(EntityUid uid, out bool holoKilled, HologramProjectedComponent? component = null)
    {
        base.DoReturnHologram(uid, out holoKilled, component);
        if (holoKilled)
            DoKillHologram(uid);
    }

    /// <summary>
    ///     Kills a Hologram after playing the visual and auditory effects.
    /// </summary>
    public void DoKillHologram(EntityUid uid, HologramComponent? holoComp = null)
    {
        if (!Resolve(uid, ref holoComp))
            return;

        var meta = MetaData(uid);
        var holoPos = Transform(uid).Coordinates;

        if (TryComp<MindContainerComponent>(uid, out var mindComp) && mindComp.Mind != null)
            _gameTicker.OnGhostAttempt(mindComp.Mind, false);

        _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(uid), coordinates: holoPos, false);
        _popup.PopupCoordinates(Loc.GetString(PopupDisappearOther, ("name", meta.EntityName)), holoPos, Filter.PvsExcept(uid), false, PopupType.MediumCaution);
        _popup.PopupCoordinates(Loc.GetString(PopupDeathSelf), holoPos, uid, PopupType.LargeCaution);
        if (TryComp<HologramServerLinkedComponent>(uid, out var holoLinkComp) && holoLinkComp.LinkedServer != null)
        {
            if (TryComp<HologramServerComponent>(holoLinkComp.LinkedServer.Value, out var serverComp))
                serverComp.LinkedHologram = null;
            holoLinkComp.LinkedServer = null;
        }

        _entityManager.QueueDeleteEntity(uid);

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid):mob} was disabled due to lack of projectors");
    }

    // private void HoloTeleport(HoloTeleportEvent args)
    // {
    // if (args.Handled) return;

    // if HoloGetProjector(args.Target, args. )
    // var transform = Transform(args.Performer);
    // if (transform.MapID != args.Target.GetMapId(EntityManager)) return;

    // _transformSystem.SetCoordinates(args.Performer, args.Target);
    // transform.AttachToGridOrMap();

    // _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));

    // _staminaSystem.TakeStaminaDamage(args.Performer, 35);

    // args.Handled = true;
    // }
    // }
}
