using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.SimpleStation14.CCVar;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Configuration;

namespace Content.Server.SimpleStation14.EndOfRoundStats.ShotsFired;

public sealed class ShotsFiredStatSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;


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
        var line = string.Empty;

        line += GenerateShotsFired(shotsFired);

        if (line != string.Empty)
            ev.AddLine("\n[color=cadetblue]" + line + "[/color]");
    }

    private string GenerateShotsFired(int shotsFired)
    {
        if (shotsFired == 0 && _config.GetCVar<bool>(SimpleStationCCVars.ShotsFiredDisplayNone))
            return Loc.GetString("eorstats-shotsfired-noshotsfired");

        if (shotsFired == 0 || shotsFired < _config.GetCVar<int>(SimpleStationCCVars.ShotsFiredThreshold))
            return string.Empty;

        return Loc.GetString("eorstats-shotsfired-amount", ("shotsFired", shotsFired));
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        shotsFired = 0;
    }
}
