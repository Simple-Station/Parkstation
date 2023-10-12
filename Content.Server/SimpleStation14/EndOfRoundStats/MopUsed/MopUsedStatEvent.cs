using Content.Shared.FixedPoint;

namespace Content.Server.SimpleStation14.EndOfRoundStats.MopUsed;

public sealed class MopUsedStatEvent : EntityEventArgs
{
    public EntityUid Mopper;
    public FixedPoint2 AmountMopped;

    public MopUsedStatEvent(EntityUid mopper, FixedPoint2 amountMopped)
    {
        Mopper = mopper;
        AmountMopped = amountMopped;
    }
}
