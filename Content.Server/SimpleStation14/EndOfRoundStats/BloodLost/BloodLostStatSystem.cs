using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Server.SimpleStation14.EndOfRoundStats.BloodLost;

public sealed class BloodLostStatSystem : EntitySystem
{
    float totalBloodLost = 0;

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

        if (totalBloodLost < 150)
            return;

        line += GenerateBloodLost(totalBloodLost);

        ev.AddLine("\n" + line);
    }

    private string GenerateBloodLost(float bloodLost)
    {
        return "[color=maroon]" + Loc.GetString("eofstats-bloodlost-total", ("bloodLost", Math.Round(bloodLost))) + "[/color]";
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        totalBloodLost = 0;
    }
}
