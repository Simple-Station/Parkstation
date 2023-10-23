using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Storage.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Pulling;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Events;

namespace Content.Shared.SimpleStation14.Holograms;

public abstract partial class SharedHologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    protected const string PopupAppearOther = "system-hologram-phasing-appear-others";
    protected const string PopupAppearSelf = "system-hologram-phasing-appear-self";
    protected const string PopupDisappearOther = "system-hologram-phasing-disappear-others";
    protected const string PopupDeathSelf = "system-hologram-phasing-death-self";
    protected const string PopupHoloInteractionFail = "system-hologram-interaction-with-others-fail";
    protected const string PopupInteractionWithHoloFail = "system-hologram-interaction-with-holo-fail";

    public const string TagHardLight = "Hardlight";
    public const string TagHoloMapped = "HoloMapped";

    public override void Initialize()
    {
        SubscribeLocalEvent<HologramComponent, InteractionAttemptEvent>(OnHoloInteractionAttempt);
        SubscribeLocalEvent<HologramComponent, GettingInteractedWithAttemptEvent>(OnInteractionWithHoloAttempt);
        SubscribeLocalEvent<HologramComponent, StoreMobInItemContainerAttemptEvent>(OnStoreInContainerAttempt);
        SubscribeLocalEvent<HologramComponent, PreventCollideEvent>(OnHoloCollide);
    }

    // Stops the Hologram from interacting with anything they shouldn't.
    private void OnHoloInteractionAttempt(EntityUid uid, HologramComponent component, InteractionAttemptEvent args)
    {
        if (HoloInteractionAllowed(args.Uid, args.Target) || args.Target == null)
            return;

        args.Cancel();

        // Send a popup to the player about the interaction, and play a sound.
        // var popup = Loc.GetString(PopupHoloInteractionFail, ("target-name", MetaData(args.Target.Value).EntityName));
        // _popup.PopupEntity(popup, args.Target.Value, args.Uid);
        // _audio.Play(component.OnSound, Filter.Entities(args.Uid), args.Target.Value, false);
    }

    // Stops everyone else from interacting with the Holograms.
    private void OnInteractionWithHoloAttempt(EntityUid uid, HologramComponent component, GettingInteractedWithAttemptEvent args)
    {
        // Allow the interaction if either of them are hardlight, or if the interactor is a Hologram.
        if (HoloInteractionAllowed(uid, args.Uid) || args.Target == null)
            return;

        args.Cancel();

        // Send a popup to the player about the interaction, and play a sound.
        // var popup = Loc.GetString(PopupInteractionWithHoloFail, ("target-name", MetaData(uid).EntityName));
        // _popup.PopupEntity(popup, uid, args.Target.Value);
        // _audio.Play(component.OnSound, Filter.Entities(args.Target.Value), uid, false);
    }

    private void OnHoloCollide(EntityUid uid, HologramComponent component, ref PreventCollideEvent args)
    {
        if (_tags.HasAnyTag(args.OtherEntity, component.CollideTags) || HoloInteractionAllowed(args.OurEntity, args.OtherEntity, component))
            return;

        args.Cancelled = true;
    }

    /// <summary>
    ///     Validates an interaction between two possibly-hologramatic entities.
    /// </summary>
    /// <param name="hologram">This should be the hologramatic entity, if one is known.</param>
    /// <param name="potential">This entity can be anything, a null value will return true.</param>
    /// <returns>True if both entities are holograms, or if either is hardlight. A null entity will return true.</returns>
    public bool HoloInteractionAllowed(EntityUid hologram, EntityUid? potential, HologramComponent? holoComp = null)
    {
        if (potential == null)
            return true;
        return _tags.HasTag(hologram, TagHardLight) || _tags.HasTag(potential.Value, TagHardLight) || Resolve(hologram, ref holoComp) == HasComp<HologramComponent>(potential);
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
