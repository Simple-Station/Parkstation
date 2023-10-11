// using static Content.Server.SimpleStation14.Hologram.HologramSystem;

namespace Content.Server.SimpleStation14.Holograms;

[RegisterComponent]
public sealed class HologramDiskComponent : Component
{
    /// <summary>
    ///     The mind stored in this Holodisk.
    /// </summary>
    [ViewVariables]
    public Mind.Mind? HoloMind = null;
}
