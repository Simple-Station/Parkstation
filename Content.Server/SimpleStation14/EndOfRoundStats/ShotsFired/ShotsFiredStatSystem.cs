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
        var line = String.Empty;

        if (shotsFired < 25)
            return;

        line += GenerateShotsFired(shotsFired);

        ev.AddLine(line);
    }

    private string GenerateShotsFired(int shotsFired)
    {
        return "\n[color=maroon]" + shotsFired + " shots were fired this round" + "[/color]";
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        shotsFired = 0;
    }
}
