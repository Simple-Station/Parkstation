using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Instruments;
using Content.Shared.GameTicking;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Instruments;

public sealed class InstrumentPlayedStatSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    Dictionary<PlayerData, TimeSpan> userPlayStats = new();

    private struct PlayerData
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
        // Gather any people currently playing istruments.
        // This part is very important :P
        // Otherwise people playing their tunes on the evac shuttle will not be counted.
        foreach (var instrument in EntityManager.EntityQuery<InstrumentComponent>().Where(i => i.InstrumentPlayer != null))
        {
            if (instrument.TimeStartedPlaying != null && instrument.InstrumentPlayer != null)
            {
                var username = instrument.InstrumentPlayer.Name;
                var entity = instrument.InstrumentPlayer.AttachedEntity;
                var name = entity != null ? MetaData((EntityUid) entity).EntityName : "Unknown";

                RaiseLocalEvent(new InstrumentPlayedStatEvent(name, (TimeSpan) (_gameTiming.CurTime - instrument.TimeStartedPlaying), username));
            }
        }

        // Continue with normal logic.
        var line = "[color=springGreen]";

        (PlayerData, TimeSpan) topPlayer = (new PlayerData(), TimeSpan.Zero);

        foreach (var (player, amountPlayed) in userPlayStats)
        {
            if (amountPlayed >= topPlayer.Item2)
                topPlayer = (player, amountPlayed);
        }

        if (topPlayer.Item2 < TimeSpan.FromMinutes(_config.GetCVar(SimpleStationCCVars.InstrumentPlayedThreshold)))
            return;
        else
            line += GenerateTopPlayer(topPlayer.Item1, topPlayer.Item2);

        ev.AddLine("\n" + line + "[/color]");
    }

    private String GenerateTopPlayer(PlayerData data, TimeSpan amountPlayed)
    {
        var line = String.Empty;

        if (data.Username != null)
            line += Loc.GetString
            (
                "eorstats-instrumentplayed-topplayer-hasusername",
                ("username", data.Username),
                ("name", data.Name),
                ("amountPlayedMinutes", Math.Round(amountPlayed.TotalMinutes))
            );
        else
            line += Loc.GetString
            (
                "eorstats-instrumentplayed-topplayer-hasnousername",
                ("name", data.Name),
                ("amountPlayedMinutes", Math.Round(amountPlayed.TotalMinutes))
            );

        return line;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        userPlayStats.Clear();
    }
}
