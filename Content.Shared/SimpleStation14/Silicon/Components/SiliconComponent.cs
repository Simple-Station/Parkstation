using Robust.Shared.GameStates;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.SimpleStation14.Silicon.Components;

/// <summary>
///     Component for defining a mob as a robot.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class SiliconComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ChargeState ChargeState = ChargeState.Full;

    [ViewVariables(VVAccess.ReadOnly)]
    public float OverheatAccumulator = 0.0f;

    /// <summary>
    ///     The owner of this component.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public new EntityUid Owner = EntityUid.Invalid;

    /// <summary>
    ///     Is the Silicon currently dead?
    /// </summary>
    public bool Dead = false;


    /// <summary>
    ///     The type of silicon this is.
    /// </summary>
    /// <remarks>
    ///     Any new types of Silicons should be added to the enum.
    /// </remarks>
    [DataField("entityType", customTypeSerializer: typeof(EnumSerializer))]
    public Enum EntityType = SiliconType.NPC;

    /// <summary>
    ///     Is this silicon battery powered?
    /// </summary>
    /// <remarks>
    ///     If true, should go along with a battery component. One will not be added automatically.
    /// </remarks>
    [DataField("batteryPowered"), ViewVariables(VVAccess.ReadWrite)]
    public bool BatteryPowered = false;

    /// <summary>
    ///     Should this silicon start charged?
    /// </summary>
    /// <remarks>
    ///     Valid values are: <see cref="StartChargedData.True"/>, <see cref="StartChargedData.False"/>, and <see cref="StartChargedData.Randomized"/>.
    /// </remarks>
    [DataField("startCharged", customTypeSerializer: typeof(EnumSerializer)), ViewVariables(VVAccess.ReadOnly)]
    public Enum StartCharged = StartChargedData.Randomized;

    /// <summary>
    ///     Multiplier for the drain rate of the silicon.
    /// </summary>
    [DataField("drainRateMulti"), ViewVariables(VVAccess.ReadWrite)]
    public float DrainRateMulti = 5.0f;


    /// <summary>
    ///     The percentages at which the silicon will enter each state.
    /// </summary>
    /// <remarks>
    ///     The Silicon will always be Full at 100%.
    ///     Setting a value to null will disable that state.
    ///     Setting Critical to 0 will cause the Silicon to never enter the dead state.
    /// </remarks>
    [DataField("chargeStateThresholdMid"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeStateThresholdMid = 0.5f;

    /// <inheritdoc cref="ChargeStateThresholdMid"/>
    [DataField("chargeStateThresholdLow"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeStateThresholdLow = 0.25f;

    /// <inheritdoc cref="ChargeStateThresholdMid"/>
    [DataField("chargeStateThresholdCritical"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeStateThresholdCritical = 0.0f;


    /// <summary>
    ///     The amount the Silicon will be slowed at each charge state.
    /// </summary>
    [DataField("speedModifierThresholds", required: true)]
    public readonly Dictionary<ChargeState, float> SpeedModifierThresholds = default!;
}
