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
        {
            ProjectedUpdate(hologram, hologramProjectedComp, frameTime);
        }
    }

    /// <summary>
    ///    Returns a hologram to its last visited projector, or kills it if the projector is invalid.
    /// </summary>
    /// <param name="hologram">The hologram to return.</param>
    /// <param name="holoKilled">True if the hologram was killed, false if it was returned.</param>
    /// <param name="holoProjectedComp">The hologram's projected component.</param>
    public virtual void DoReturnHologram(EntityUid hologram, out bool holoKilled, HologramProjectedComponent? holoProjectedComp = null)
    {
        holoKilled = false;

        if (!Resolve(hologram, ref holoProjectedComp))
            return;

        if (!IsHoloProjectorValid(hologram, holoProjectedComp.CurProjector, 0, false)) // If their last visited Projector is invalid...
        {
            if (!TryGetHoloProjector(hologram, out holoProjectedComp.CurProjector, 0, false)) // Check for a new one, ignoring occlusion...
            {
                holoKilled = true; // And if none is found, kill the hologram.
                return;
            }
        }

        MoveHologramToProjector(hologram, holoProjectedComp.CurProjector!.Value);

        _adminLogger.Add(LogType.Mind, LogImpact.Low,
            $"{ToPrettyString(hologram):mob} was returned to projector {ToPrettyString(holoProjectedComp.CurProjector.Value):entity}");
    }

    /// <summary>
    /// Tests for the nearest projector to a set of coords.
    /// </summary>
    /// <param name="coords">The coords to perform the check from.</param>
    /// <param name="result">The UID of the projector, or null if no projectors are found.</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <returns>Returns true if a projector is found, false if not.</returns>
    public bool TryGetHoloProjector(MapCoordinates coords, [NotNullWhen(true)] out EntityUid? result, float range = 18f, bool occlude = true)
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
            if (!IsHoloProjectorValid(coords, nearProj.Value, range, occlude)) continue;
            result = nearProj.Value;
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool TryGetHoloProjector(EntityUid uid, out EntityUid? result, float range = 18f, bool occlude = true)
    {
        return TryGetHoloProjector(Transform(uid).MapPosition, out result, range, occlude);
    }

    /// <summary>
    ///    Returns the best projector for a hologram to use.
    /// </summary>
    /// <remarks>
    ///   Defaults to the hologram's current projector, if it has one.
    /// </remarks>
    /// <param name="hologram">The hologram to find a projector for.</param>
    /// <param name="projector">The UID of the projector, or null if no projectors are found.</param>
    /// <param name="holoProjectedComp">The hologram's projected component.</param>
    public bool TryGetWorkingProjector(EntityUid hologram, [NotNullWhen(true)] out EntityUid? projector, HologramProjectedComponent? holoProjectedComp = null)
    {
        projector = null;

        if (!Resolve(hologram, ref holoProjectedComp))
            return false;

        if (IsHoloProjectorValid(hologram, holoProjectedComp.CurProjector))
        {
            projector = holoProjectedComp.CurProjector!;
            return true;
        }

        return TryGetHoloProjector(hologram, out projector);
    }

    /// <summary>
    ///     Tests if a projector is valid for a given hologram.
    /// </summary>
    /// <param name="hologram">The hologram to check for, or its position.</param>
    /// <param name="projector">The projector to compare on, or its position.</param>
    /// <param name="range">The max range to allow. Ignored if occlude is false.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <returns>True if the projector is within range, and unoccluded to the hologram. Otherwise, false.</returns>
    public bool IsHoloProjectorValid(EntityUid hologram, EntityUid? projector, float range = 18f, bool occlude = true)
    {
        return _entityManager.EntityExists(projector) && (!occlude || projector.Value.InRangeUnOccluded(hologram, range));
    }

    /// <inheritdoc/>
    public bool IsHoloProjectorValid(EntityUid hologram, MapCoordinates projector, float range = 18f, bool occlude = true)
    {
        return !occlude || projector.InRangeUnOccluded(hologram, range);
    }

    /// <inheritdoc/>
    public bool IsHoloProjectorValid(MapCoordinates hologram, EntityUid? projector, float range = 18f, bool occlude = true)
    {
        return _entityManager.EntityExists(projector) && (!occlude || projector.Value.InRangeUnOccluded(hologram, range));
    }

    /// <inheritdoc/>
    public bool IsHoloProjectorValid(MapCoordinates hologram, MapCoordinates projector, float range = 18f, bool occlude = true)
    {
        return !occlude || projector.InRangeUnOccluded(hologram, range);
    }

    /// <summary>
    ///    Moves a hologram to a projector.
    /// </summary>
    /// <param name="hologram">The hologram to move.</param>
    /// <param name="projector">The projector to move it to.</param>
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

        // Plays the appearing effects.
        if (!_timing.InPrediction)
        {
            _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", hologram);
            _popup.PopupEntity(Loc.GetString(PopupAppearOther, ("name", meta.EntityName)), hologram, Filter.PvsExcept(hologram), false, PopupType.Medium);
            _popup.PopupEntity(Loc.GetString(PopupAppearSelf, ("name", meta.EntityName)), hologram, hologram, PopupType.Large);
        }
    }

    /// <inheritdoc/>
    public void MoveHologramToProjector(EntityUid hologram, EntityUid projector)
    {
        MoveHologram(hologram, Transform(projector).Coordinates);
    }

    protected bool ProjectedUpdate(EntityUid hologram, HologramProjectedComponent hologramProjectedComp, float frameTime)
    {
        if (TryGetWorkingProjector(hologram, out var nearProj, hologramProjectedComp))
        {
            hologramProjectedComp.GraceTimer = hologramProjectedComp.GracePeriod;
            hologramProjectedComp.CurProjector = nearProj;
            return true;
        }

        // If none is found, starts counting...
        if (hologramProjectedComp.GraceTimer > 0)
        {
            hologramProjectedComp.GraceTimer -= frameTime;
            return true;
        }

        // Attempts to return the hologram if their time is up.
        DoReturnHologram(hologram, out _);
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
