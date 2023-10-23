using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.SimpleStation14.Holograms.Components;

/// <summary>
///     Marks that this Hologram is projected from cameras, or some other hologram projector source.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HologramProjectedComponent : Component
{
    /// <summary>
    ///     A list of tags to check for on projectors, to determine if they're valid.
    /// </summary>
    [DataField("validProjectorTags", customTypeSerializer: typeof(PrototypeIdListSerializer<TagPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public List<string> ValidProjectorTags = new();

    /// <summary>
    ///     A timer for a grace period before the Holo is returned, to allow for moving through doors.
    /// </summary>
    [DataField("gracePeriod"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan GracePeriod = TimeSpan.FromSeconds(0.1f);

    /// <summary>
    ///     The maximum range from a projector a Hologram can be before they're returned.
    /// </summary>
    [DataField("projectorRange"), ViewVariables(VVAccess.ReadWrite)]
    public float ProjectorRange = 18f;

    /// <summary>
    ///     The prototype of the effect to spawn for the Hologram's projection, assuming <see cref="DoProjectionEffect"/> is true.
    /// </summary>
    [DataField("effectPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EffectPrototype = "EffectHologramProjectionBeam";

    /// <summary>
    ///     Whether or not the Hologram should have a visual projection effect.
    /// </summary>
    [DataField("doProjectionEffect"), ViewVariables(VVAccess.ReadWrite)]
    public bool DoProjectionEffect = true;

    /// <summary>
    ///     The current projector the hologram is connected to.
    /// </summary>
    /// <remarks>
    ///     Note that this may not be a valid projector, as it is left set to the last projector the Hologram was in range of during the grace period.
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public EntityUid? CurProjector;

    /// <summary>
    ///     If set, the Hologram will only be able to be projected from this projector, simply ignoring all others.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProjectorOverride;

    /// <summary>
    ///     Whether or not the Hologram is currently in the range of a projector.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField] // TODO: Probably remove this and just sync the projector then determine this client side?
    public bool CurrentlyInProjector = false;

    /// <summary>
    ///     The point at which a Hologram will be sent back to their last projector or killed, based on when they were last in the range of one.
    /// </summary>
    /// <remarks>
    ///     Note that THIS WILL NOT BE SET TO NULL. If a hologram enters a projector, this value will be left alone and simply be innacurate.
    ///     Do not rely on it.
    /// </remarks>
    // [AutoNetworkedField]
    public TimeSpan VanishTime = TimeSpan.Zero;

    /// <summary>
    ///     The UID of the entity for the Hologram's visual projection effect.
    ///     Client side only.
    /// </summary>
    public EntityUid? EffectEntity = null;
}
