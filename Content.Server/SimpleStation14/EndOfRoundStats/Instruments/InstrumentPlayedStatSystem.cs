using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Instruments;

public sealed class InstrumentPlayedStatSystem : EntitySystem
{
    Dictionary<PlayerData, TimeSpan> userPlayStats = new();

    int currentPlace = 1; // For making the top players.

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

        if (userPlayStats.Count == 0)
        {
            line += "\n\n[color=red]" + "No vibes were had this round..." + "[/color]";
        }
        else
        {
            (PlayerData, TimeSpan) topPlayer = (new PlayerData(), TimeSpan.Zero);

            foreach (var (player, amountPlayed) in userPlayStats)
            {
                if (amountPlayed >= topPlayer.Item2)
                    topPlayer = (player, amountPlayed);
            }

            line += GenerateTopPlayer(topPlayer.Item1, topPlayer.Item2);
        }

        ev.AddLine(line);
    }

    private String GenerateTopPlayer(PlayerData data, TimeSpan amountPlayed)
    {
        var line = String.Empty;

        if (amountPlayed > TimeSpan.Zero)
        {
            if (data.Username == null)
                line += "\n\n" + "The master of vibes this round was " + data.Name + " \n" + "having played their tunes for " + amountPlayed + "!";
            else
                line += "\n\n" + "The master of vibes this round was " + data.Username + " as " + data.Name + " \n" + "having played their tunes for " + amountPlayed + "!";

            currentPlace++;
        }

        return line;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        userPlayStats.Clear();
        currentPlace = 1;
    }
}
