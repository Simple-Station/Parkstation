using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Interaction.Helpers;
using Robust.Shared.Map;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Storage.Components;
using Content.Shared.Pulling.Components;
using Content.Shared.Database;
using Content.Shared.SimpleStation14.Holograms.Components;

namespace Content.Shared.SimpleStation14.Holograms;

public abstract partial class SharedHologramSystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = _entityManager.EntityQueryEnumerator<HologramProjectedComponent>();
        while (query.MoveNext(out var hologram, out var hologramProjectedComp))
            ProjectedUpdate(hologram, hologramProjectedComp, frameTime);
    }

    /// <summary>
    ///     Returns a hologram to its last visited projector, or kills it if the projector is invalid.
    /// </summary>
    /// <param name="hologram">The hologram to return.</param>
    /// <param name="holoKilled">True if the hologram was killed, false if it was returned.</param>
    /// <param name="holoProjectedComp">The hologram's projected component.</param>
    public virtual void DoReturnHologram(EntityUid hologram, out bool holoKilled, HologramProjectedComponent? holoProjectedComp = null)
    {
        holoKilled = false;

        if (!Resolve(hologram, ref holoProjectedComp))
            return;

        if (!IsHoloProjectorValid(hologram, holoProjectedComp.CurProjector, 0, false)) // If their last visited Projector is invalid ignoring occlusion...
        {
            if (!TryGetHoloProjector(hologram, holoProjectedComp.ProjectorRange, out holoProjectedComp.CurProjector, holoProjectedComp))
            {
                var killedEvent = new HologramKillAttemptEvent();
                RaiseLocalEvent(hologram, ref killedEvent);
                if (killedEvent.Cancelled)
                    return;

                RaiseLocalEvent(hologram, new HologramKilledEvent());
                holoKilled = true; // And if none is found and it doesn't get cancelled, kill the hologram.
                return; // The killing is dealt with server-side, due to mind component.
            }
        }

        // The two if statements above set the current projector, and return if it's null, so we know it's not null moving forward.

        var returnedEvent = new HologramReturnAttemptEvent();
        RaiseLocalEvent(hologram, ref returnedEvent);
        if (returnedEvent.Cancelled)
            return;

        RaiseLocalEvent(hologram, new HologramReturnedEvent(holoProjectedComp.CurProjector!.Value));

        MoveHologramToProjector(hologram, holoProjectedComp.CurProjector!.Value);

        _adminLogger.Add(LogType.Mind, LogImpact.Low,
            $"{ToPrettyString(hologram):mob} was returned to projector {ToPrettyString(holoProjectedComp.CurProjector.Value):entity}");
    }

    /// <summary>
    ///     Tests for the nearest projector to a set of coords.
    /// </summary>
    /// <param name="coords">The coords to perform the check from.</param>
    /// <param name="result">The UID of the projector, or null if no projectors are found.</param>
    /// <param name="allowedTags">A list of tags to check for on projectors, to determine if they're valid.</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <returns>Returns true if a projector is found, false if not.</returns>
    public bool TryGetHoloProjector(MapCoordinates coords, float range, [NotNullWhen(true)] out EntityUid? result, List<string>? allowedTags = null, bool occlude = true)
    {
        result = null;

        // Sort all projectors in distance increasing order.
        var nearProjList = new SortedList<float, EntityUid>();

        var query = _entityManager.EntityQueryEnumerator<HologramProjectorComponent>();
        while (query.MoveNext(out var projector, out _))
        {
            var dist = (_transform.GetWorldPosition(projector) - coords.Position).LengthSquared();
            nearProjList.TryAdd(dist, projector);
        }

        // Find the nearest, valid projector.
        foreach (var nearProj in nearProjList)
        {
            if (!IsHoloProjectorValid(coords, nearProj.Value, range, occlude))
                continue;
            result = nearProj.Value;
            return true;
        }
        return false;
    }

    /// <remarks>
    ///     This takes into consideration any ProjectorOverride the hologram may have.
    /// </remarks>
    /// <inheritdoc cref="TryGetHoloProjector(MapCoordinates, float, out EntityUid?, List{string}?, bool)"/>
    public bool TryGetHoloProjector(EntityUid uid, float range, [NotNullWhen(true)] out EntityUid? result, HologramProjectedComponent? projectedComp = null, bool occlude = true)
    {
        result = null;

        if (!Resolve(uid, ref projectedComp))
            return false;

        if (projectedComp.ProjectorOverride != null) // Check for Component-set overrides.
        {
            if (IsHoloProjectorValid(uid, projectedComp.ProjectorOverride, range, occlude))
            {
                result = projectedComp.ProjectorOverride;
                return true;
            }
            return false;
        }

        var projectorEvent = new HologramGetProjectorEvent(); // Check for Event-set overrides.
        RaiseLocalEvent(uid, ref projectorEvent);
        if (projectorEvent.Override)
        {
            result = projectorEvent.ProjectorOverride;
            return projectorEvent.ProjectorOverride != null;
        }

        // Otherwise, we simply check for the nearest projector, considering any tags it requires.
        return TryGetHoloProjector(Transform(uid).MapPosition, range, out result, projectedComp.ValidProjectorTags, occlude);
    }

    /// <summary>
    ///     Tests if a projector is valid for a given hologram.
    /// </summary>
    /// <param name="hologram">The hologram to check for, or its position.</param>
    /// <param name="projector">The projector to compare on, or its position.</param>
    /// <param name="range">The max range to allow. Ignored if occlude is false.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <returns>True if the projector is within range, and unoccluded to the hologram. Otherwise, false.</returns>
    public bool IsHoloProjectorValid(EntityUid hologram, EntityUid? projector, float range = 18f, bool occlude = true, HologramProjectedComponent? projectedComp = null)
    {
        if (!Resolve(hologram, ref projectedComp))
            return false;
        return IsHoloProjectorValid(Transform(hologram).MapPosition, projector, range, occlude, projectedComp.ValidProjectorTags);
    }

    /// <inheritdoc cref="IsHoloProjectorValid(EntityUid, EntityUid?, float, bool, HologramProjectedComponent?)"/>
    /// <param name="allowedTags">A list of tags to check for on projectors, to determine if they're valid. Usually found on the Holo's <see cref="HologramProjectedComponent"/>.</param>
    public bool IsHoloProjectorValid(MapCoordinates hologram, EntityUid? projector, float range = 18f, bool occlude = true, List<string>? allowedTags = null)
    {
        if (projector == null || !Exists(projector.Value))
            return false;

        if (allowedTags != null && !_tags.HasAnyTag(projector.Value, allowedTags))
            return false;

        if (occlude && !projector.Value.InRangeUnOccluded(hologram, range))
            return false;

        return true;
    }

    /// <summary>
    ///     Moves a hologram to a new location.
    /// </summary>
    /// <remarks>
    ///     Does no validation for any projectors before moving.
    /// </remarks>
    /// <param name="hologram">The hologram to move.</param>
    /// <param name="projector">The projector to move it to, or the projector's position.</param>
    public void MoveHologram(EntityUid hologram, EntityCoordinates projector)
    {
        // Stops any pulling goin on.
        if (TryComp<SharedPullableComponent>(hologram, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(pullable);

        if (TryComp<SharedPullerComponent>(hologram, out var pulling) && pulling.Pulling != null &&
            TryComp<SharedPullableComponent>(pulling.Pulling.Value, out var subjectPulling))
            _pulling.TryStopPull(subjectPulling);

        // Plays the vanishing effects.
        var meta = MetaData(hologram);

        if (!_timing.InPrediction)
        {
            var holoPos = Transform(hologram).Coordinates;
            _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(hologram), coordinates: holoPos, false);
            _popup.PopupCoordinates(Loc.GetString(PopupDisappearOther, ("name", meta.EntityName)), holoPos, Filter.PvsExcept(hologram), false, PopupType.MediumCaution);
        }

        // Does the do.
        _transform.SetCoordinates(hologram, projector);
        _transform.AttachToGridOrMap(hologram);

        // Plays the appearing effects.
        if (!_timing.InPrediction)
        {
            _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", hologram);
            _popup.PopupEntity(Loc.GetString(PopupAppearOther, ("name", meta.EntityName)), hologram, Filter.PvsExcept(hologram), false, PopupType.Medium);
            _popup.PopupEntity(Loc.GetString(PopupAppearSelf, ("name", meta.EntityName)), hologram, hologram, PopupType.Large);
        }
    }

    /// <inheritdoc cref="MoveHologram(EntityUid, EntityCoordinates)"/>
    public void MoveHologramToProjector(EntityUid hologram, EntityUid projector)
    {
        MoveHologram(hologram, Transform(projector).Coordinates);
    }

    protected bool ProjectedUpdate(EntityUid hologram, HologramProjectedComponent hologramProjectedComp, float frameTime)
    {
        if (TryGetHoloProjector(hologram, hologramProjectedComp.ProjectorRange, out var nearProj, hologramProjectedComp)) // Checks for a projector in range.
        {
            hologramProjectedComp.CurProjector = nearProj;
            hologramProjectedComp.CurrentlyInProjector = true;
            Dirty(hologramProjectedComp);
            return true;
        }

        // If none is found, and they were in the range of a projector during the last check, we set the time they'll be disappeared at.
        if (hologramProjectedComp.CurrentlyInProjector)
        {
            hologramProjectedComp.CurrentlyInProjector = false;
            hologramProjectedComp.VanishTime = _timing.CurTime + hologramProjectedComp.GracePeriod;
        }

        if (hologramProjectedComp.VanishTime > _timing.CurTime)
        {
            Dirty(hologramProjectedComp);
            return true;
        }

        // Attempts to return the hologram if their time is up.
        DoReturnHologram(hologram, out _);
        Dirty(hologramProjectedComp);
        return false;
    }

    // Forbid holograms from going inside anything. Osmosised from Nyano :)
    private void OnStoreInContainerAttempt(EntityUid uid, HologramComponent component, ref StoreMobInItemContainerAttemptEvent args)
    {
        if (HasComp<HologramProjectedComponent>(uid))
        {
            DoReturnHologram(uid, out _);
            args.Cancelled = true;
            args.Handled = true;
        }
    }
}
