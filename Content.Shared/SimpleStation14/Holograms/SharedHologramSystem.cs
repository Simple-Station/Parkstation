using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Damage;
using Content.Shared.Item;
using Content.Shared.Tag;
using Content.Shared.Interaction.Helpers;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Serialization;
using Robust.Shared.Containers;
using Content.Shared.Computer;
using Content.Shared.Pulling.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Pulling;

namespace Content.Shared.SimpleStation14.Hologram;

public class SharedHologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    private const string DiskSlot = "holo_disk";

    public override void Initialize()
    {
        SubscribeLocalEvent<HologramComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeNetworkEvent<HologramReturnEvent>(HoloReturn);
    }

    // Stops the Hologram from interacting with anything they shouldn't.
    private void OnInteractionAttempt(EntityUid uid, HologramComponent component, InteractionAttemptEvent args)
    {
        if (args.Target == null)
            return;

        if (HasComp<TransformComponent>(args.Target) && !HasComp<UnremoveableComponent>(args.Target)
            && !_tagSystem.HasAnyTag(args.Target.Value, "Hardlight")) args.Cancel();
    }

    /// <summary>
    ///     Handles returning a Hologram to their last visited projector,
    ///     then to the nearest, finally killing them if none are found.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>
    public void HoloReturn(HologramReturnEvent args)
    {
        var uid = args.Uid;
        var component = _entityManager.GetComponent<HologramComponent>(uid);
        var meta = _entityManager.GetComponent<MetaDataComponent>(uid);
        var holoPos = _entityManager.GetComponent<TransformComponent>(uid).Coordinates;

        var popupAppearOther = Loc.GetString("system-hologram-phasing-appear-others", ("name", meta.EntityName));
        var popupAppearSelf = Loc.GetString("system-hologram-phasing-appear-self");
        var popupDisappearOther = Loc.GetString("system-hologram-phasing-disappear-others", ("name", meta.EntityName));
        var popupDeathSelf = Loc.GetString("system-hologram-phasing-death-self");

        // If the Hologram's last projector isn't valid, try to find a new one.
        if (component.CurProjector != null && _entityManager.TryGetComponent<TransformComponent>(component.CurProjector, out var _))
        {
            var curProjEvent = new HologramProjectorValidEvent(uid, component.CurProjector.Value);
            RaiseNetworkEvent(curProjEvent);
            if (!curProjEvent.Valid)
            {
                var getProj = new HologramGetProjectorEvent(uid, false);
                component.CurProjector = getProj.Projector;
            }
        }
        else
        {
            var getProj = new HologramGetProjectorEvent(uid, false);
            component.CurProjector = getProj.Projector;
        }

        // If the Hologram's last projector is still invalid, kill them.
        if (component.CurProjector == EntityUid.Invalid)
        {

            RaiseNetworkEvent(new HologramKillEvent(uid));
            return;
        }

        _entityManager.TryGetComponent<TransformComponent>(component.CurProjector, out var transfComp);
        Popup.PopupCoordinates(popupDisappearOther, holoPos, Filter.PvsExcept(uid), false, PopupType.MediumCaution);
        _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(uid), coordinates: holoPos, false);

        // Preapre to move holo
        if (TryComp<SharedPullableComponent>(uid, out var pullable) && pullable.BeingPulled) _pulling.TryStopPull(pullable);
        if (TryComp<SharedPullerComponent>(uid, out var pulling) && pulling.Pulling != null &&
            TryComp<SharedPullableComponent>(pulling.Pulling.Value, out var subjectPulling)) _pulling.TryStopPull(subjectPulling);

        // Move holo
        Transform(uid).Coordinates = _entityManager.GetComponent<TransformComponent>((EntityUid) component.CurProjector).Coordinates;

        Popup.PopupEntity(popupAppearOther, uid, Filter.PvsExcept((EntityUid) uid), false, PopupType.Medium);
        Popup.PopupEntity(popupAppearSelf, uid, uid, PopupType.Large);
        _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", uid);

        _adminLogger.Add(LogType.Unknown, LogImpact.Low,
            $"{ToPrettyString(uid):mob} was returned to projector {ToPrettyString((EntityUid) component.CurProjector):entity}");
    }
}


// [Serializable, NetSerializable]
// public sealed class HoloTeleportEvent : EntityEventArgs
// {
//     public readonly EntityUid Uid;
//     public readonly List<EntityUid> Lights;

//     public ShadekinDarkenEvent(EntityUid uid, List<EntityUid> lights)
//     {
//         Uid = uid;
//         Lights = lights;
//     }
// }
