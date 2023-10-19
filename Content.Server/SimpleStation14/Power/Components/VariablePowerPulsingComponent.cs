namespace Content.Server.SimpleStation14.Power.Components;

/// <summary>
///     Registers an item as currently experiencing a pulse in power usage.
/// </summary>
[RegisterComponent]
public sealed class VariablePowerPulsingComponent : Component
{
    /// <summary>
    ///     The time at which the pulse is done.
    /// </summary>
    public TimeSpan PulseDoneTime = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Whether the device should return to an active or inactive state after the pulse is done.
    /// </summary>
    public bool NonPulseState = false;
}
