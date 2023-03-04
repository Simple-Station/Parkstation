using Content.Shared.Examine;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Network;
using Content.Shared.IdentityManagement;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystemPowerSystem : EntitySystem
    {
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<ShadekinComponent, ComponentInit>(OnInit);
        }

        private void OnExamine(EntityUid uid, ShadekinComponent component, ExaminedEvent args)
        {
            if (args.IsInDetailsRange && !_net.IsClient)
            {
                var powerType = GetLevelName(component.PowerLevel);

                if (args.Examined == args.Examiner)
                {
                    args.PushMarkup(Loc.GetString("shadekin-power-examined-self",
                        ("power", GetLevelInt(component.PowerLevel)),
                        ("powerMax", component.PowerLevelMax),
                        ("powerType", powerType)
                    ));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("shadekin-power-examined-other",
                        ("target", Identity.Entity(uid, EntityManager)),
                        ("powerType", powerType)
                    ));
                }
            }
        }

        private void OnInit(EntityUid uid, ShadekinComponent component, ComponentInit args)
        {
            if (component.PowerLevel <= ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min])
                SetPowerLevel(component, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Great]);
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            // Update power level for all shadekin
            foreach (var component in EntityManager.EntityQuery<ShadekinComponent>())
            {
                UpdatePowerLevel(component, frameTime);

                if (!component.PowerLevelGainEnabled) continue;

                if (component.PowerLevel <= ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min] + 1f)
                {
                    RaiseLocalEvent(new ShadekinBlackeyeEvent(component.Owner));
                    RaiseNetworkEvent(new ShadekinBlackeyeEvent(component.Owner));
                }
            }
        }



        /// <param name="PowerLevel">The current power level.</param>
        /// <returns>The name of the power level.</returns>
        public string GetLevelName(float PowerLevel)
        {
            // Placeholders
            var powerType = Loc.GetString("shadekin-power-okay");
            ShadekinPowerThreshold result = ShadekinPowerThreshold.Min;
            var value = ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Max];

            // Find the highest threshold that is lower than the current power level
            foreach (var threshold in ShadekinComponent.PowerThresholds)
            {
                if (threshold.Value <= value && threshold.Value >= PowerLevel)
                {
                    result = threshold.Key;
                    value = threshold.Value;
                }
            }

            // Get the name of the threshold
            // TODO: Localize this
            switch (result)
            {
                case ShadekinPowerThreshold.Max:
                    powerType = Loc.GetString("shadekin-power-max");
                    break;
                case ShadekinPowerThreshold.Great:
                    powerType = Loc.GetString("shadekin-power-great");
                    break;
                case ShadekinPowerThreshold.Good:
                    powerType = Loc.GetString("shadekin-power-good");
                    break;
                case ShadekinPowerThreshold.Okay:
                    powerType = Loc.GetString("shadekin-power-okay");
                    break;
                case ShadekinPowerThreshold.Tired:
                    powerType = Loc.GetString("shadekin-power-tired");
                    break;
                case ShadekinPowerThreshold.Min:
                    powerType = Loc.GetString("shadekin-power-min");
                    break;

                default:
                    powerType = Loc.GetString("shadekin-power-okay");
                    break;
            }

            // Return the name of the threshold
            return powerType;
        }

        // Very dumb
        /// <remarks> For viewing purposes. </remarks>
        /// <param name="PowerLevel">The current power level.</param>
        /// <returns>Power level as an integer.</returns>
        public int GetLevelInt(float PowerLevel)
        {
            return (int) Math.Round(PowerLevel);
        }

        /// <summary>
        ///     Updates the power level of a shadekin based on an amount of seconds.
        /// </summary>
        /// <param name="component">The shadekin component.</param>
        /// <param name="frameTime">The time since the last update in seconds.</param>
        public void UpdatePowerLevel(ShadekinComponent component, float frameTime)
        {
            // Check if power gain is enabled
            if (!component.PowerLevelGainEnabled) return;

            // Calculate new power level using the formula (P = P + t * G * M)
            var newPowerLevel = component.PowerLevel + frameTime * component.PowerLevelGain * component.PowerLevelGainMultiplier;

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            SetPowerLevel(component, newPowerLevel);
        }

        /// <summary>
        ///     Sets the power level of a shadekin.
        /// </summary>
        /// <param name="component">The shadekin component.</param>
        /// <param name="newPowerLevel">The new power level.</param>
        public void SetPowerLevel(ShadekinComponent component, float newPowerLevel)
        {
            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            component._powerLevel = newPowerLevel;
        }
    }
}
