namespace Content.Server.SimpleStation14.EndOfRoundStats.BloodLost;

public sealed class BloodLostStatEvent : EntityEventArgs
{
    public float BloodLost;

    public BloodLostStatEvent(float bloodLost)
    {
        BloodLost = bloodLost;
    }
}
