namespace Content.Server.SimpleStation14.Silicon.Death;

/// <summary>
///     Marks a Silicon as becoming incapacitated when they run out of battery charge.
/// </summary>
/// <remarks>
///     Uses the Silicon System's charge states to do so, so make sure they're a battery powered Silicon.
/// </remarks>
[RegisterComponent]
public sealed class SiliconDownOnDeadComponent : Component
{
    /// <summary>
    ///     Is this Silicon currently dead?
    /// </summary>
    public bool Dead { get; set; } = false;
}
