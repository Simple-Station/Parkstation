using Content.Shared.SimpleStation14.Hologram;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Hologram;

/// <summary>
/// Raised when a hologram is being returned to its last visited projector.
/// </summary>
[Serializable, NetSerializable]
public sealed class HologramReturnEvent : EntityEventArgs
{
    public EntityUid Uid;

    public HologramReturnEvent(EntityUid uid)
    {
        Uid = uid;
    }
}

/// <summary>
/// Raised when a hologram is being killed and removed from the game world.
/// </summary>
[Serializable, NetSerializable]
public sealed class HologramKillEvent : EntityEventArgs
{
    public EntityUid Uid;

    public HologramKillEvent(EntityUid uid)
    {
        Uid = uid;
    }
}

/// <summary>
/// Raised to return a bool if a given projector is valid for a given hologram.
/// </summary>
[Serializable, NetSerializable]
public sealed class HologramProjectorTestEvent : EntityEventArgs
{
    public EntityUid Hologram;
    public EntityUid Projector;
    public bool CanProject = false;

    public HologramProjectorTestEvent(EntityUid hologram, EntityUid projector)
    {
        Hologram = hologram;
        Projector = projector;
    }
}

/// <summary>
/// Raised to return the nearest projector to a given hologram.
/// </summary>
[Serializable, NetSerializable]
public sealed class HologramGetProjectorEvent : EntityEventArgs
{
    public EntityUid Hologram;
    public bool Occlude;
    public float Range;
    public EntityUid Projector;

    public HologramGetProjectorEvent(EntityUid hologram, bool occlude = true, float range = 18f)
    {
        Hologram = hologram;
        Occlude = occlude;
        Range = range;
    }
}
