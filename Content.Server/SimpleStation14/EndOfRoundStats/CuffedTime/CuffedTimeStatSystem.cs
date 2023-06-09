using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.GameTicking;
using Content.Shared.SimpleStation14.CCVar;
using Content.Shared.SimpleStation14.EndOfRoundStats.CuffedTime;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.EndOfRoundStats.CuffedTime;

public sealed class CuffedTimeStatSystem : EntitySystem
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

        SubscribeLocalEvent<CuffableComponent, CuffedTimeStatEvent>(OnUncuffed);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnUncuffed(EntityUid uid, CuffableComponent component, CuffedTimeStatEvent args)
    {
        string? username = null;

        if (EntityManager.TryGetComponent<MindComponent>(uid, out var mindComponent) &&
            mindComponent.Mind != null &&
            mindComponent.Mind.Session != null)
            username = mindComponent.Mind.Session.Name;

        var playerData = new PlayerData
        {
            Name = MetaData(uid).EntityName,
            Username = username
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
        // Gather any people currently cuffed.
        // Otherwise people cuffed on the evac shuttle will not be counted.
        var query = EntityQueryEnumerator<CuffableComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.CuffedTime != null)
                RaiseLocalEvent(uid, new CuffedTimeStatEvent(_gameTiming.CurTime - component.CuffedTime.Value));
        }

        // Continue with normal logic.
        var line = "[color=cadetblue]";

        (PlayerData, TimeSpan) topPlayer = (new PlayerData(), TimeSpan.Zero);

        foreach (var (player, amountPlayed) in userPlayStats)
        {
            if (amountPlayed >= topPlayer.Item2)
                topPlayer = (player, amountPlayed);
        }

        if (topPlayer.Item2 < TimeSpan.FromMinutes(_config.GetCVar<int>(SimpleStationCCVars.CuffedTimeThreshold)))
            return;
        else
            line += GenerateTopPlayer(topPlayer.Item1, topPlayer.Item2);

        ev.AddLine("\n" + line + "[/color]");
    }

    private String GenerateTopPlayer(PlayerData data, TimeSpan timeCuffed)
    {
        var line = String.Empty;

        if (data.Username != null)
            line += Loc.GetString
            (
                "eorstats-cuffedtime-hasusername",
                ("username", data.Username),
                ("name", data.Name),
                ("timeCuffedMinutes", Math.Round(timeCuffed.TotalMinutes))
            );
        else
            line += Loc.GetString
            (
                "eorstats-cuffedtime-nousername",
                ("name", data.Name),
                ("timeCuffedMinutes", Math.Round(timeCuffed.TotalMinutes))
            );

        return line;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        userPlayStats.Clear();
    }
}
