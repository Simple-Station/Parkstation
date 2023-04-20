namespace Content.Server.SimpleStation14.Silicon.Components;

[RegisterComponent]
public sealed class BloodstreamFillerComponent : Component
{
    /// <summary>
    /// The name of the volume to refill.
    /// </summary>
    /// <remarks>
    /// Should match the <see cref="SolutionContainerComponent"/> name or otherwise.
    /// </remarks>
    [DataField("solution"), ViewVariables(VVAccess.ReadWrite)]
    public string Solution { get; } = "filler";

    /// <summary>
    ///     The amount of reagent that this silicon filler will fill with at most.
    /// </summary>
    [DataField("amount"), ViewVariables(VVAccess.ReadWrite)]
    public float Amount = 100.0f;
}
