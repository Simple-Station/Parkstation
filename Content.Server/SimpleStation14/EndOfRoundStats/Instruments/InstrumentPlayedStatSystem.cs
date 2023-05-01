using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Instruments;

public sealed class InstrumentPlayedStatSystem : EntitySystem
{
    Dictionary<PlayerData, TimeSpan> userPlayStats = new();

    public struct PlayerData
    {
        public String Name;
        public String? Username;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InstrumentPlayedStatEvent>(OnInstrumentPlayed);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnInstrumentPlayed(InstrumentPlayedStatEvent args)
    {
        var playerData = new PlayerData
        {
            Name = args.Player,
            Username = args.Username
        };

        if (userPlayStats.ContainsKey(playerData))
        {
            userPlayStats[playerData] += args.Duration;
            return;
        }

        userPlayStats.Add(playerData, args.Duration);
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        var line = String.Empty;

        (PlayerData, TimeSpan) topPlayer = (new PlayerData(), TimeSpan.Zero);

        foreach (var (player, amountPlayed) in userPlayStats)
        {
            if (amountPlayed >= topPlayer.Item2)
                topPlayer = (player, amountPlayed);
        }

        if (topPlayer.Item2 < TimeSpan.FromMinutes(8))
            line += "[color=red]" + "No vibes were had this round..." + "[/color]";
        else
            line += GenerateTopPlayer(topPlayer.Item1, topPlayer.Item2);

        ev.AddLine("\n" + line);
    }

    private String GenerateTopPlayer(PlayerData data, TimeSpan amountPlayed)
    {
        var line = String.Empty;

        if (data.Username == null)
            line += "The master of vibes this round was " + data.Name + " \n" + "having played their tunes for " + amountPlayed.Minutes + " minutes!";
        else
            line += "The master of vibes this round was " + data.Username + " as " + data.Name + " \n" + "having played their tunes for " + amountPlayed.Minutes + " minutes!";

        return line;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        userPlayStats.Clear();
    }
}
