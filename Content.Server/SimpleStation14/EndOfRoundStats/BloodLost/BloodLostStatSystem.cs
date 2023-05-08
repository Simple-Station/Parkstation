using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Content.Shared.SimpleStation14.CCVar;
using Content.Shared.FixedPoint;

namespace Content.Server.SimpleStation14.EndOfRoundStats.BloodLost;

public sealed class BloodLostStatSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;

    FixedPoint2 totalBloodLost = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodLostStatEvent>(OnBloodLost);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnBloodLost(BloodLostStatEvent args)
    {
        totalBloodLost += args.BloodLost;
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        var line = String.Empty;

        if (totalBloodLost < _config.GetCVar<float>(SimpleStationCCVars.BloodLostThreshold))
            return;

        line += GenerateBloodLost(totalBloodLost);

        ev.AddLine("\n" + line);
    }

    private string GenerateBloodLost(FixedPoint2 bloodLost)
    {
        return "[color=maroon]" + Loc.GetString("eorstats-bloodlost-total", ("bloodLost", bloodLost.Int())) + "[/color]";
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        totalBloodLost = 0;
    }
}
