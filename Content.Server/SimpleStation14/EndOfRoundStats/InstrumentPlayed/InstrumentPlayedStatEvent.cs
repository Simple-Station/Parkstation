namespace Content.Server.SimpleStation14.EndOfRoundStats.Instruments;

public sealed class InstrumentPlayedStatEvent : EntityEventArgs
{
    public String Player;
    public TimeSpan Duration;
    public String? Username;

    public InstrumentPlayedStatEvent(String player, TimeSpan duration, String? username)
    {
        Player = player;
        Duration = duration;
        Username = username;
    }
}
