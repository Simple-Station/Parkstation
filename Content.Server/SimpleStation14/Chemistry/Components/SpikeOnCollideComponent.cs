namespace Content.Server.SimpleStation14.Chemistry.Components;

[RegisterComponent]
public sealed class SpikeOnCollideComponent : Component
{
    [DataField("solution")] public string Solution = default!;
}
