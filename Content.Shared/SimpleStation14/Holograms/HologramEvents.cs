using Content.Shared.SimpleStation14.Hologram;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Hologram;

// /// <summary>
// /// Raised when a hologram is being returned to its last visited projector.
// /// </summary>
// public sealed class HologramReturnEvent : EntityEventArgs
// {
//     public HologramComponent Component;

//     public HologramReturnEvent(HologramComponent component)
//     {
//         Component = component;
//     }
// }

// /// <summary>
// /// Raised when a hologram is being killed and removed from the game world.
// /// </summary>
// public sealed class HologramKillEvent : EntityEventArgs
// {
//     public HologramComponent Component;

//     public HologramKillEvent(HologramComponent component)
//     {
//         Component = component;
//     }
// }

/// <summary>
/// Raised when a hologram is being killed and removed from the game world.
/// </summary>
[Serializable, NetSerializable]
public sealed class HologramDiskInsertedEvent : EntityEventArgs
{
    public EntityUid Uid;
    public HologramServerComponent ServerComponent;

    public HologramDiskInsertedEvent(EntityUid uid, HologramServerComponent serverComponent)
    {
        Uid = uid;
        ServerComponent = serverComponent;
    }
}
