namespace Content.Server.SimpleStation14.Holograms;

/// <summary>
///     Marks this entity as storing a hologram's data in it, for use in a <see cref="HologramServerComponent"/>.
/// </summary>
[RegisterComponent]
public sealed class HologramDiskComponent : Component
{
    /// <summary>
    ///     The mind stored in this Holodisk.
    /// </summary>
    [ViewVariables]
    public Mind.Mind? HoloMind = null;
}
