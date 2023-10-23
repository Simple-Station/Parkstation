using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.SimpleStation14.Holograms;

/// <summary>
///     Marks the entity as a being made of light.
///     Details determined by sister components.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed class HologramComponent : Component
{
    /// <summary>
    ///     The sound to play when the Hologram is turned on.
    /// </summary>
    [DataField("onSound"), ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier OnSound = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg");

    /// <summary>
    ///     The sound to play when the Hologram is turned off.
    /// </summary>
    [DataField("offSound"), ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier OffSound = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg");

    /// <summary>
    ///     A list of tags for the Hologram to collide with, assuming they're not hardlight.
    /// </summary>
    /// <remarks>
    ///     This should generally include the 'Wall' tag.
    /// </remarks>
    [DataField("collideTags", customTypeSerializer: typeof(PrototypeIdListSerializer<TagPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public List<string> CollideTags = new() { "Wall", "Airlock" };
}
