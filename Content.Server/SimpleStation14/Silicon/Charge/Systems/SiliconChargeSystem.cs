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
using Content.Server.Body.Components;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class SiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe for init on entities with both SiliconComponent and BatteryComponent
        SubscribeLocalEvent<BatteryComponent, ComponentInit>(OnBatteryInit);
    }


    private void OnBatteryInit(EntityUid uid, BatteryComponent component, ComponentInit args)
    {
        if (!EntityManager.TryGetComponent<SiliconComponent>(uid, out var siliconComp) ||
            !siliconComp.BatteryPowered)
            return;

        component.MaxCharge *= _random.NextFloat(0.85f, 1.15f);

        var batteryLevelFill = component.MaxCharge;

        if (siliconComp.StartCharged.Equals(StartChargedData.Randomized))
            batteryLevelFill *= _random.NextFloat(0.40f, 0.80f);

        component.CurrentCharge = batteryLevelFill;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // For each siliconComp entity with a battery component, drain their charge.
        var query = EntityQueryEnumerator<SiliconComponent, BatteryComponent>();
        while (query.MoveNext(out var silicon, out var siliconComp, out var batteryComp))
        {
            // If the silicon is dead, skip it.
            if (_mobStateSystem.IsDead(silicon))
                continue;

            var drainRate = 10 * (siliconComp.DrainRateMulti);

            // All multipliers will be subtracted by 1, and then added together, and then multiplied by the drain rate. This is then added to the base drain rate.
            // This is to stop exponential increases, while still allowing for less-than-one multipliers.
            var drainRateFinalAddi = 0f;

            // TODO: Devise a method of adding multis where other systems can alter the drain rate.
            // Maybe use something similar to refreshmovespeedmodifiers, where it's stored in the component.
            // Maybe it doesn't matter, and stuff should just use static drain?

            drainRateFinalAddi += SiliconHeatEffects(silicon, frameTime) - 1;

            // Ensures that the drain rate is at least 10% of normal,
            // and would allow at least 4 minutes of life with a max charge, to prevent cheese.
            drainRate += Math.Clamp(drainRateFinalAddi, drainRate * -0.9f, batteryComp.MaxCharge / 240);

            // Drain the battery.
            batteryComp.UseCharge(frameTime * drainRate);

            // Figure out the current state of the Silicon.
            var chargePercent = batteryComp.CurrentCharge / batteryComp.MaxCharge;

            ChargeState currentState;
            switch (chargePercent)
            {
                case var x when x > siliconComp.ChargeStateThresholdMid:
                    currentState = ChargeState.Full;
                    break;
                case var x when x > siliconComp.ChargeStateThresholdLow:
                    currentState = ChargeState.Mid;
                    break;
                case var x when x > siliconComp.ChargeStateThresholdCritical:
                    currentState = ChargeState.Low;
                    break;
                case var x when x > 0 || siliconComp.ChargeStateThresholdCritical == 0:
                    currentState = ChargeState.Critical;
                    break;
                default:
                    currentState = ChargeState.Dead;
                    break;
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
        if (!EntityManager.TryGetComponent<TemperatureComponent>(silicon, out var temperComp) ||
            !EntityManager.TryGetComponent<ThermalRegulatorComponent>(silicon, out var thermalComp))
        {
            return 0;
        }

        var siliconComp = EntityManager.GetComponent<SiliconComponent>(silicon);

        // If the Silicon is hot, drain the battery faster, if it's cold, drain it slower, capped.
        var upperThresh = thermalComp.NormalBodyTemperature + (thermalComp.ThermalRegulationTemperatureThreshold);
        var lowerThresh = thermalComp.NormalBodyTemperature - (thermalComp.ThermalRegulationTemperatureThreshold);
        var upperThreshHalf = thermalComp.NormalBodyTemperature + (thermalComp.ThermalRegulationTemperatureThreshold * 0.5f);

        // Check if the silicon is in a hot environment.
        if (temperComp.CurrentTemperature > upperThreshHalf)
        {
            // Divide the current temp by the max comfortable temp capped to 4, then add that to the multiplier.
            var hotTempMulti = Math.Min(temperComp.CurrentTemperature / upperThreshHalf, 4);
            // If the silicon is hot enough, it has a chance to catch fire.
            FlammableComponent? flamComp = null;

            siliconComp.OverheatAccumulator += frameTime;
            if (siliconComp.OverheatAccumulator >= 5)
            {
                siliconComp.OverheatAccumulator =- 5;

                if (EntityManager.TryGetComponent<FlammableComponent>(silicon, out flamComp) &&
                    temperComp.CurrentTemperature > temperComp.HeatDamageThreshold &&
                    !flamComp.OnFire &&
                    _random.Prob(Math.Clamp(temperComp.CurrentTemperature / (upperThresh * 5), 0.001f, 0.9f)))
                {
                    _flammableSystem.Ignite(silicon, flamComp);
                }
                else if ((flamComp == null || !flamComp.OnFire) &&
                        _random.Prob(Math.Clamp(temperComp.CurrentTemperature / (upperThresh), 0.001f, 0.75f)))
                {
                    _popup.PopupEntity(Loc.GetString("silicon-system-overheating"), silicon, silicon, PopupType.SmallCaution);
                }
            }

            return hotTempMulti;
        }

        // Check if the silicon is in a cold environment.
        if (temperComp.CurrentTemperature < thermalComp.NormalBodyTemperature)
        {
            var coldTempMulti = (0.5f + (temperComp.CurrentTemperature / thermalComp.NormalBodyTemperature) * 0.5f);

            return coldTempMulti;
        }

        return 0;
    }
}
