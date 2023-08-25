using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.SimpleStation14.Power.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.Power.Systems;

public sealed class RandomBatteryFillSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomBatteryChargeComponent, ComponentInit>(OnBatteryInit);
    }

    private void OnBatteryInit(EntityUid uid, RandomBatteryChargeComponent component, ComponentInit args)
    {
        var batteryComp = Comp<BatteryComponent>(uid);
        DebugTools.AssertNotNull(batteryComp);

        if (batteryComp == null)
            return;

        var (minMaxMod, maxMaxMod) = component.BatteryMaxMinMax;
        var (minChargeMod, maxChargeMod) = component.BatteryChargeMinMax;

        var newMax = batteryComp.MaxCharge * _random.NextFloat(minMaxMod, maxMaxMod);
        float newCharge;

        if (component.BasedOnMaxCharge)
            newCharge = newMax * _random.NextFloat(minChargeMod, maxChargeMod);
        else
            newCharge = batteryComp.CurrentCharge * _random.NextFloat(minChargeMod, maxChargeMod);

        _battery.SetMaxCharge(uid, newMax);
        _battery.SetCharge(uid, newCharge);
    }
}
