using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Holograms.Components;

/// <summary>
///     Marks that this Hologram requires a server to generate it.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed class HologramProjectedComponent : Component
{
    /// <summary>
    ///     A timer for a grace period before the Holo is returned, to allow for moving through doors.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("gracePeriod")]
    public float GracePeriod = 0.5f;

    /// <summary>
    ///    The current projector the hologram is connected to.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CurProjector;

    /// <summary>
    ///     A timer for a grace period before the Holo is returned, to allow for moving through doors.
    /// </summary>
    public float GraceTimer = 0.5f;
}
