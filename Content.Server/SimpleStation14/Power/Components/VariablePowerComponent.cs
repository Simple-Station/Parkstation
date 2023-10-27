using System.ComponentModel.DataAnnotations;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Server.SimpleStation14.Power.Components;

/// <summary>
///     Defines that the given entity should take a variable amount of power, depending on if it's being used.
/// </summary>
[RegisterComponent]
public sealed class VariablePowerComponent : Component
{
    /// <summary>
    ///     The amount of power to draw when set to active.
    /// </summary>
    [DataField("activeLoad"), ViewVariables(VVAccess.ReadWrite)]
    public float ActiveLoad = 500;

    /// <summary>
    ///     The multiplier for how much power to draw when idle.
    /// </summary>
    [DataField("powerIdleMulti"), ViewVariables(VVAccess.ReadWrite)]
    public float IdleMulti = 0.1f;

    /// <summary>
    ///     The amount of power to draw when told to 'pulse'.
    /// </summary>
    [DataField("pulseLoad"), Required, ViewVariables(VVAccess.ReadWrite)]
    public float PulseLoad = 5000;

    /// <summary>
    ///     The amount of time this device pulses for.
    /// </summary>
    [DataField("pulseTime"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan PulseTime = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Whether the device is currently active.
    /// </summary>
    [DataField("active"), ViewVariables(VVAccess.ReadOnly)]
    public bool Active = false;
}
