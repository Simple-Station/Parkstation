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

namespace Content.Shared.SimpleStation14.Hologram;

public class SharedHologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HologramServerComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
    }

    // Stops the Hologram from interacting with anything they shouldn't.
    private void OnInteractionAttempt(EntityUid uid, HologramComponent component, InteractionAttemptEvent args)
    {
        if (args.Target == null)
            return;

        if (TryComp<DamageableComponent>(args.Target, out var dmg) && dmg.DamageContainerID == "Biological")
            args.Cancel();

        if (HasComp<ItemComponent>(args.Target) && !HasComp<UnremoveableComponent>(args.Target)
            && !_tagSystem.HasAnyTag(args.Target.Value, "Hardlight"))
            args.Cancel();
    }

    /// <summary>
    /// Handle adding keys to the ignition, give stuff the InVehicleComponent so it can't be picked
    /// up by people not in the vehicle.
    /// </summary>
    private void OnEntInserted(EntityUid uid, HologramServerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != DiskSlot ||
            !_entityManager.TryGetComponent<HologramDiskComponent>(args.Entity, out var disk)) return;

        TryHoloGenerate(component.Owner, disk.HoloData!, _entityManager.GetComponent<CloningPodComponent>(component.Owner));
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
