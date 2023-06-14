using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.BloodstreamFiller;

[Serializable, NetSerializable]
public sealed class BloodstreamFillerDoAfterEvent : DoAfterEvent
{
    [DataField("overfill")]
    public readonly bool Overfill = false;

    private BloodstreamFillerDoAfterEvent()
    {
    }
    public BloodstreamFillerDoAfterEvent(bool overfill)
    {
        Overfill = overfill;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
