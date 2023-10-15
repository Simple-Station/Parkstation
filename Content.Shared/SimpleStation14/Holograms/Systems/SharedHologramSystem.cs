using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Storage.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Pulling;
using Robust.Shared.Timing;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Holograms;

public abstract partial class SharedHologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    protected const string PopupAppearOther = "system-hologram-phasing-appear-others";
    protected const string PopupAppearSelf = "system-hologram-phasing-appear-self";
    protected const string PopupDisappearOther = "system-hologram-phasing-disappear-others";
    protected const string PopupDeathSelf = "system-hologram-phasing-death-self";
    protected const string PopupInteractionFail = "system-hologram-light-interaction-fail";

    public override void Initialize()
    {
        SubscribeLocalEvent<HologramComponent, InteractionAttemptEvent>(OnHoloInteractionAttempt);
        SubscribeLocalEvent<InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<HologramComponent, StoreMobInItemContainerAttemptEvent>(OnStoreInContainerAttempt);
    }

    // Stops the Hologram from interacting with anything they shouldn't.
    private void OnHoloInteractionAttempt(EntityUid uid, HologramComponent component, InteractionAttemptEvent args)
    {
        if (args.Target == null)
            return;

        if (HasComp<TransformComponent>(args.Target) && !HasComp<UnremoveableComponent>(args.Target)
            && !_tagSystem.HasAnyTag(args.Target.Value, "Hardlight", "Softlight")) args.Cancel();
    }

    // Stops everyone else from interacting with the Holograms.
    private void OnInteractionAttempt(InteractionAttemptEvent args)
    {
        if (args.Target == null || _tagSystem.HasAnyTag(args.Uid, "Hardlight", "Softlight") ||
            _entityManager.TryGetComponent<HologramComponent>(args.Uid, out var _))
            return;

        if (_tagSystem.HasAnyTag(args.Target.Value, "Softlight") && !_tagSystem.HasAnyTag(args.Target.Value, "Hardlight"))
        {
            args.Cancel();

            // Send a popup to the player about the interaction, and play a sound.
            var meta = _entityManager.GetComponent<MetaDataComponent>(args.Target.Value);
            var popup = Loc.GetString(PopupInteractionFail, ("item", meta.EntityName));
            var sound = "/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg";
            _popup.PopupEntity(popup, args.Target.Value, Filter.Entities(args.Uid), false);
            _audio.Play(sound, Filter.Entities(args.Uid), args.Uid, false);
        }
    }
}
// public struct HoloData
// {
//     [DataField("type")]
//     public HoloType Type { get; set; }

//     [DataField("isHardlight")]
//     public bool IsHardlight { get; set; }

//     public HoloData(HoloType type, bool isHardlight = false)
//     {
//         Type = type;
//         IsHardlight = isHardlight;
//     }
// }


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
