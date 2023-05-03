using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Server.SimpleStation14.EndOfRoundStats.ShotsFired;

public sealed class ShotsFiredStatSystem : EntitySystem
{
    int shotsFired = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnShotFired);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnShotFired(EntityUid _, GunComponent __, AmmoShotEvent args)
    {
        shotsFired++;
    }
    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        var line = "\n[color=cadetblue]";

        if (shotsFired < 25 && shotsFired != 0)
            return;

        line += GenerateShotsFired(shotsFired);

        ev.AddLine(line + "[/color]");
    }

    private string GenerateShotsFired(int shotsFired)
    {
        if (shotsFired == 0)
            return Loc.GetString("eofstats-shotsfired-noshotsfired");

        return Loc.GetString("eofstats-shotsfired-amount", ("shotsFired", shotsFired));
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        shotsFired = 0;
    }
}
