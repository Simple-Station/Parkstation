namespace Content.Shared.SimpleStation14.Hologram;

[RegisterComponent]
public sealed class HologramComponent : Component
{
    [ViewVariables]
    public EntityUid? LinkedServer;

    [ViewVariables]
    public EntityUid? CurProjector;

    // Counter
    [DataField("accumulator")]
    public float Accumulator = 2f;
}
