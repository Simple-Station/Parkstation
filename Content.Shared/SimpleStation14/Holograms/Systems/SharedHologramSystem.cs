using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Interaction.Helpers;
using Robust.Shared.Map;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Storage.Components;
using Content.Shared.Pulling.Components;
using Content.Shared.Database;
using Content.Shared.Administration.Logs;
using Content.Shared.Pulling;
using Robust.Shared.Timing;

namespace Content.Shared.SimpleStation14.Holograms;

public abstract class SharedHologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly GameTiming _timing = default!;

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

    // Forbid holograms from going inside anything. Osmosised from Nyano :)
    private void OnStoreInContainerAttempt(EntityUid uid, HologramComponent component, ref StoreMobInItemContainerAttemptEvent args)
    {
        args.Cancelled = true;
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = _entityManager.EntityQueryEnumerator<HologramComponent>();
        while (query.MoveNext(out var hologram, out var hologramComp))
        {
            TryGetHoloProjector(hologram, out var nearProj);

            if (nearProj == null)
            {
                if (hologramComp.GracePeriod == 0)
                {
                    hologramComp.GracePeriod -= frameTime;
                    continue;
                }
                DoReturnHologram(hologram, out _);
                continue;
            }

            hologramComp.GracePeriod = 0.24f;
            hologramComp.CurProjector = nearProj;
        }
    }

    /// <summary>
    ///     Handles returning a Hologram to their last visited projector,
    ///     then to the nearest, finally killing them if none are found.
    /// </summary>
    public virtual void DoReturnHologram(EntityUid uid, out bool holoKilled)
    {
        var component = _entityManager.GetComponent<HologramComponent>(uid);
        var meta = MetaData(uid);
        var holoPos = Transform(uid).Coordinates;

        holoKilled = false;

        if (component.CurProjector == null) // TODO:HOLO Check if the projector works.
        {
            TryGetHoloProjector(uid, out component.CurProjector);
        }

        if (component.CurProjector == null) // If the Hologram still doesn't have a working projector, kill it. // TODO:HOLO Check if the projector works.
        {
            holoKilled = true;
            return;
        }

        // TODO:HOLO Make this all part of a SetHoloProjector function.
        _entityManager.TryGetComponent<TransformComponent>(component.CurProjector, out var transfComp);
        if (_timing.IsFirstTimePredicted)
        {
            _popup.PopupCoordinates(Loc.GetString(PopupDisappearOther, ("name", meta.EntityName)), holoPos, Filter.PvsExcept(uid), false, PopupType.MediumCaution);
            _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(uid), coordinates: holoPos, false);
        }

        // Prepare to move holo
        if (TryComp<SharedPullableComponent>(uid, out var pullable) && pullable.BeingPulled) _pulling.TryStopPull(pullable);
        if (TryComp<SharedPullerComponent>(uid, out var pulling) && pulling.Pulling != null &&
            TryComp<SharedPullableComponent>(pulling.Pulling.Value, out var subjectPulling)) _pulling.TryStopPull(subjectPulling);

        // Move holo
        _transform.SetCoordinates(uid, Transform(component.CurProjector.Value).Coordinates);

        if (_timing.IsFirstTimePredicted)
        {
            _popup.PopupEntity(Loc.GetString(PopupAppearOther, ("name", meta.EntityName)), uid, Filter.PvsExcept(uid), false, PopupType.Medium);
            _popup.PopupEntity(Loc.GetString(PopupAppearSelf, ("name", meta.EntityName)), uid, uid, PopupType.Large);
            _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", uid);
        }

        _adminLogger.Add(LogType.Unknown, LogImpact.Low,
            $"{ToPrettyString(uid):mob} was returned to projector {ToPrettyString((EntityUid) component.CurProjector):entity}");
    }

    /// <summary>
    /// Tests for the nearest projector to a set of coords.
    /// </summary>
    /// <param name="coords">Coords to test from.</param>
    /// <param name="mapId">Map being tested on.</param>
    /// <param name="result">The UID of the projector, or null if no projectors are found.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <returns>Returns true if a projector is found, false if not.</returns>
    public bool TryGetHoloProjector(Vector2 coords, MapId mapId, [NotNullWhen(true)] out EntityUid? result, bool occlude = true, float range = 18f)
    {
        result = null;

        var xformQuery = GetEntityQuery<TransformComponent>();

        // sort all entities in distance increasing order
        var nearProjList = new SortedList<float, EntityUid>();

        var query = _entityManager.EntityQueryEnumerator<HologramProjectorComponent>();
        while (query.MoveNext(out var projector, out var projComp))
        {
            if (!xformQuery.TryGetComponent(projector, out var compXform) || compXform.MapID != mapId)
                continue;

            var dist = (_transform.GetWorldPosition(compXform!, xformQuery) - coords).LengthSquared();
            nearProjList.TryAdd(dist, projector);
        }

        foreach (var nearProj in nearProjList)
        {
            if (occlude && !nearProj.Value.InRangeUnOccluded(new MapCoordinates(coords, mapId), range)) continue;
            result = nearProj.Value;
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool TryGetHoloProjector(EntityUid uid, [NotNullWhen(true)] out EntityUid? result, bool occlude = true, float range = 18f)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var transform = _entityManager.GetComponent<TransformComponent>(uid);
        var playerPos = _transform.GetWorldPosition(transform, xformQuery);

        return TryGetHoloProjector(playerPos, transform.MapID, out result, occlude, range);
    }
}

public enum HoloTypeEnum
{
    /// <summary>
    ///    A hologram projected from a projector.
    /// </summary>
    Projected,
    /// <summary>
    ///     A hologram tied to the internal storage of a lightbee.
    /// </summary>
    Lightbee,
    /// <summary>
    ///     A hologram that
    /// </summary>
    Standalone,
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
