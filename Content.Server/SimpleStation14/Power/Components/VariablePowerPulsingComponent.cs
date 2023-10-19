namespace Content.Server.SimpleStation14.Power.Components;

[RegisterComponent]
public sealed class VariablePowerPulsingComponent : Component
{
    public TimeSpan PulseDoneTime = TimeSpan.FromSeconds(1);
}
