using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Repairable;

public abstract class SharedRepairableSystem : EntitySystem
{
    [Serializable, NetSerializable]
    public sealed class RepairFinishedEvent : SimpleDoAfterEvent // Parkstation-IPC // Protected? PROTECTED?? The absolute gall!
    {
    }
}

