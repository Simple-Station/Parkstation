using Content.Shared.DeviceNetwork;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Client.SimpleStation14.Hologram;

[RegisterComponent]
public sealed class HoloProjectorComponent : Component
{
    [ViewVariables]
    public bool Active { get; set; } = true;
}
