// using static Content.Server.SimpleStation14.Hologram.HologramSystem;

namespace Content.Server.SimpleStation14.Hologram;

[RegisterComponent]
public sealed class HologramDiskComponent : Component
{
    // [ViewVariables]
    // public HoloDataEntry? HoloData = null;

    // [DataField("active"), ViewVariables(VVAccess.ReadWrite)]
    // public bool Active = true;

    [ViewVariables]
    public Mind.Mind? HoloData = null;

    [DataField("active"), ViewVariables(VVAccess.ReadWrite)]
    public bool Active = true;
}
