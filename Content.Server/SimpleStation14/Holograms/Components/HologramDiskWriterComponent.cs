namespace Content.Server.SimpleStation14.Holograms.Components;

[RegisterComponent]
public sealed class HologramDiskWriterComponent : Component
{
    [DataField("diskSlot")]
    public string DiskSlot = "disk_slot";
}
