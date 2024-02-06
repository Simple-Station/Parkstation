using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.SimpleStation14.Holograms.Components;

/// <summary>
///     For any items that should generate a 'dummy' hologram when inserted as a holo disk.
///     Mostly intended for jokes and gaffs, but could be used for useful AI entities as well.
/// </summary>
[RegisterComponent]
public sealed class HologramDiskDummyComponent : Component
{
    /// <summary>
    ///     The prototype to spawn when this disk is inserted into a server.
    /// </summary>
    [DataField("prototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string HoloPrototype = default!;
}
