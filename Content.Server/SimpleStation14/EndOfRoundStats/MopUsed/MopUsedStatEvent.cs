using Content.Shared.FixedPoint;

namespace Content.Server.SimpleStation14.EndOfRoundStats.MopUsed;

public sealed class MopUsedStatEvent : EntityEventArgs
{
    public String Mopper;
    public FixedPoint2 AmountMopped;
    public String? Username;

    public MopUsedStatEvent(String mopper, FixedPoint2 amountMopped, String? username)
    {
        Mopper = mopper;
        AmountMopped = amountMopped;
        Username = username;
    }
}
