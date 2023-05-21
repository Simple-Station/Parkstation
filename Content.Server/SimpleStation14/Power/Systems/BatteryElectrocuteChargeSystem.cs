using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Shared.Electrocution;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Power.Systems;

public sealed class BatteryElectrocuteChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, ElectrocutedEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(EntityUid uid, BatteryComponent battery, ElectrocutedEvent args)
    {
        if (args.ShockDamage == null || args.ShockDamage <= 0)
            return;

        var damage = args.ShockDamage.Value * args.SiemensCoefficient;
        var charge = Math.Min(damage * 300f, battery.MaxCharge * 0.25f) * _random.NextFloat(0.75f, 1.25f);

        battery.CurrentCharge += charge;

        if (!_gameTiming.InPrediction)
            return;

        var message = Loc.GetString("battery-electrocute-charge");

        _popup.PopupEntity(message, uid, uid);
    }
}
