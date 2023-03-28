using Content.Shared.StepTrigger.Components;

namespace Content.Server.SimpleStation14.Silicon;

[RegisterComponent]
public sealed class SiliconChargerComponent : Component
{
    /// <summary>
    ///     The multiplier for the charge rate.
    ///     For reference, an IPC drains at 50.
    /// </summary>
    [DataField("chargeMulti"), ViewVariables(VVAccess.ReadWrite)]
    public float ChargeMulti = 150f;

    /// <summary>
    ///     The temperature the charger will stop heating up at.
    /// </summary>
    [DataField("targetTemp"), ViewVariables(VVAccess.ReadWrite)]
    public float TargetTemp = 365f;

    /// <summary>
    ///     The number of entities that can be stood on a charger at once.
    [DataField("maxEntities"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxEntities = 1;


    /// <summary>
    ///     The list of entities currently stood on a charger.
    ///     Used specifically for chargers with the <see cref="StepTriggerComponent"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> PresentEntities = new List<EntityUid>();

    [ViewVariables(VVAccess.ReadWrite)]
    public float warningAccumulator = 0f;
}
