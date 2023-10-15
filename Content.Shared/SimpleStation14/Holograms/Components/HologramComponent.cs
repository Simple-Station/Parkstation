using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.SimpleStation14.Holograms;

/// <summary>
///     Marks the entity as a being made of light.
///     Details determined by sister components.
/// </summary>
[RegisterComponent]
public sealed class HologramComponent : Component
{
    /// <summary>
    ///     Is this a hardlight Hologram.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isHardlight")]
    public bool IsHardlight = false;
}
