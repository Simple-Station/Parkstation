using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Silicon;

[Serializable, NetSerializable]
public sealed class BatteryDrinkerDoAfterEvent : SimpleDoAfterEvent
{
    public BatteryDrinkerDoAfterEvent()
    {
    }
}
