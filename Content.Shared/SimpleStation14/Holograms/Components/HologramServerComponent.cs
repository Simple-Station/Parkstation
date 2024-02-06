namespace Content.Shared.SimpleStation14.Holograms;

/// <summary>
///     Marks an entity as being capable of generating a hologram by inserting a <see cref="HologramDiskComponent"/> into it.
/// </summary>
[RegisterComponent]
public sealed class HologramServerComponent : Component
{
    [DataField("diskSlot")]
    public string? DiskSlot;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? LinkedHologram;
}
