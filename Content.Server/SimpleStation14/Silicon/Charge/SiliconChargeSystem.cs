using Robust.Shared.Random;
using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Server.Power.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class SiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    static string popupOverheating = Loc.GetString("silicon-system-overheating");

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe for init on entities with both SiliconComponent and BatteryComponent
        SubscribeLocalEvent<BatteryComponent, ComponentInit>(OnBatteryInit);
    }


    private void OnBatteryInit(EntityUid uid, BatteryComponent component, ComponentInit args)
    {
        if (!EntityManager.TryGetComponent<SiliconComponent>(uid, out var siliconComp) ||
            !siliconComp.BatteryPowered ||
            !siliconComp.StartCharged)
        {
            return;
        }

        component.CurrentCharge = component.MaxCharge;
    }

    public override void Update(float frameTime)
    {

        base.Update(frameTime);

        // For each siliconComp entity with a battery component, drain their charge.
        foreach (var (siliconComp, batteryComp) in EntityManager.EntityQuery<SiliconComponent, BatteryComponent>())
        {
            if (siliconComp.Owner == EntityUid.Invalid)
                continue;

            var silicon = siliconComp.Owner;

            // If the silicon is not battery powered, or is dead, skip it.
            if (!siliconComp.BatteryPowered ||
                _mobStateSystem.IsDead(silicon))
                continue;

            var drainRate = 10 * (siliconComp.DrainRateMulti);

            // All multipliers will be added together
            // and then divided by the added weight, before applying to the drain rate.
            var drainRateFinalMulti = 0f;

            //TODO: Make this actually support more than one multipler. Math is hard ;-;
            // Adding more than a couple, or a few smaller multipliers to this will cause exponential drain.
            // Fix this before doing that.
            drainRateFinalMulti += SiliconHeatEffects(silicon, frameTime);

            if (drainRateFinalMulti != 0)
            {
                drainRate *= drainRateFinalMulti;
            }

            Logger.Warning($"Drain rate: {drainRate}");

            // Drain the battery.
            batteryComp.UseCharge(frameTime * drainRate);

            // Figure out the current state of the Silicon.
            var currentState = ChargeState.Dead;
            var chargePercent = batteryComp.CurrentCharge / batteryComp.MaxCharge;

            if (chargePercent == 0 && siliconComp.ChargeStateThresholdCritical != 0)
            {
                currentState = ChargeState.Dead;
            }
            else if (chargePercent <= siliconComp.ChargeStateThresholdCritical)
            {
                currentState = ChargeState.Critical;
            }
            else if (chargePercent <= siliconComp.ChargeStateThresholdLow)
            {
                currentState = ChargeState.Low;
            }
            else if (chargePercent < siliconComp.ChargeStateThresholdMid)
            {
                currentState = ChargeState.Mid;
            }
            else if (chargePercent >= siliconComp.ChargeStateThresholdMid)
            {
                currentState = ChargeState.Full;
            }

            // Check if anything needs to be updated.
            if (currentState != siliconComp.ChargeState)
            {
                siliconComp.ChargeState = currentState;

                RaiseLocalEvent<SiliconChargeStateUpdateEvent>(silicon, new SiliconChargeStateUpdateEvent(currentState));

                _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(silicon);
            }
        }
    }

    private float SiliconHeatEffects(EntityUid silicon, float frameTime)
    {
        if (!EntityManager.TryGetComponent<TemperatureComponent>(silicon, out var temperComp))
        {
            Logger.Warning("Silicon has no temperature component!");

            return 0;
        }

        // If the Silicon is hot, drain the battery faster, if it's cold, drain it slower, capped.

        // Check if the silicon is in a hot environment.
        if (temperComp.CurrentTemperature > 300)
        {
            Logger.Warning("Silicon is hot!");

            // Divide the current temp by 300 capped to 4, then add that to the multiplier.
            var hotTempMulti = Math.Min(temperComp.CurrentTemperature / 300, 4);

            // If the silicon is hot enough, it has a chance to catch fire.
            FlammableComponent? flamComp = null;

            if (EntityManager.TryGetComponent<FlammableComponent>(silicon, out flamComp) &&
                temperComp.CurrentTemperature > 360 &&
                !flamComp.OnFire &&
                _random.Prob(0.05f * frameTime + (temperComp.CurrentTemperature / 3600)))
            {
                Logger.Warning("Silicon is on fire!");

                _flammableSystem.Ignite(silicon, flamComp);
            }
            else if (temperComp.CurrentTemperature > 300 &&
                    (flamComp == null || !flamComp.OnFire) &&
                    _random.Prob(0.085f * frameTime + (temperComp.CurrentTemperature / 36000)))
            {
                Logger.Warning("Silicon is overheating!");

                _popup.PopupEntity(popupOverheating, silicon, silicon, PopupType.SmallCaution);
            }
            Logger.Warning($"Hot temp multi: {hotTempMulti}");

            return hotTempMulti;
        }

        // Check if the silicon is in a cold environment.
        if (temperComp.CurrentTemperature < 280)
        {
            Logger.Warning("Silicon is cold!");

            var coldTempMulti = (0.5f + temperComp.CurrentTemperature / 560);
            return coldTempMulti;
        }

        Logger.Warning("Silicon is normal temp!");

        return 0;
    }
}
