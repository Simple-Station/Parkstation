using Content.Shared.Storage.Components;
using Content.Shared.StepTrigger.Components;
using Robust.Shared.Audio;

namespace Content.Shared.SimpleStation14.Silicon;

[RegisterComponent]
public sealed class SiliconChargerComponent : Component
{
    /// <summary>
    ///     Is the charger currently active?
    /// </summary>
    public bool Active = false;

    /// <summary>
    ///     The currently playing audio stream.
    /// </summary>
    public IPlayingAudioStream? SoundStream { get; set; }

    /// <summary>
    ///     Counter for handing out warnings to burning entities.
    /// </summary>
    public float warningAccumulator = 0f;


    /// <summary>
    ///     The sound to play when the charger is active.
    /// </summary>
    [DataField("soundLoop")]
    public SoundSpecifier SoundLoop = new SoundPathSpecifier("/Audio/Machines/microwave_loop.ogg");

    /// <summary>
    ///     The multiplier for the charge rate.
    ///     For reference, an IPC drains at 50.
    /// </summary>
    [DataField("chargeMulti"), ViewVariables(VVAccess.ReadWrite)]
    public float ChargeMulti = 50f;

    /// <summary>
    ///     The minimum size of a battery to be charged.
    /// </summary>
    /// <remarks>
    ///     Charging a battery too small will detonate it, becoming more likely as it fills.
    /// </remarks>
    [DataField("minChargeSize"), ViewVariables(VVAccess.ReadWrite)]
    public int MinChargeSize = 1000;


    /// <summary>
    ///     The temperature the charger will stop heating up at.
    /// </summary>
    /// <remarks>
    ///     Used specifically for chargers with the <see cref="SharedEntityStorageComponent"/>.
    /// </remarks>
    [DataField("targetTemp"), ViewVariables(VVAccess.ReadWrite)]
    public float TargetTemp = 373.15f;


    /// <summary>
    ///     The list of entities currently stood on a charger.
    /// </summary>
    /// <remarks>
    ///     Used specifically for chargers with the <see cref="StepTriggerComponent"/>.
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> PresentEntities = new List<EntityUid>();

    /// <summary>
    ///     The number of entities that can be stood on a charger at once.
    /// <summary>
    /// <remarks>
    ///     Used specifically for chargers with the <see cref="StepTriggerComponent"/>.
    /// </remarks>
    [DataField("maxEntities"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxEntities = 1;
}
