using Content.Shared.Alert;
using Content.Shared.Rounding;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using System.Threading.Tasks;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public class ShadekinPowerSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;


        /// <param name="PowerLevel">The current power level.</param>
        /// <returns>The name of the power level.</returns>
        public string GetLevelName(float PowerLevel)
        {
            // Placeholders
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

            var powerDictionary = new Dictionary<ShadekinPowerThreshold, string>
            {
                {ShadekinPowerThreshold.Max, Loc.GetString("shadekin-power-max")},
                {ShadekinPowerThreshold.Great, Loc.GetString("shadekin-power-great")},
                {ShadekinPowerThreshold.Good, Loc.GetString("shadekin-power-good")},
                {ShadekinPowerThreshold.Okay, Loc.GetString("shadekin-power-okay")},
                {ShadekinPowerThreshold.Tired, Loc.GetString("shadekin-power-tired")},
                {ShadekinPowerThreshold.Min, Loc.GetString("shadekin-power-min")}
            };

            // Get the name of the threshold
            powerDictionary.TryGetValue(result, out var powerType);
            if (powerType == null) powerType = Loc.GetString("shadekin-power-okay");

            // Return the name of the threshold
            return powerType;
        }

        /// <summary>
        ///    Sets the alert level of a shadekin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="PowerLevel">The current power level.</param>
        public void UpdateAlert(EntityUid uid, bool enabled, float? PowerLevel = null)
        {
            if (!enabled || PowerLevel == null)
            {
                _alertsSystem.ClearAlert(uid, AlertType.ShadekinPower);
                return;
            }

            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to update alert of entity without shadekin component.");
                throw new InvalidOperationException("Tried to update alert of entity without shadekin component.");
            }

            // Get the power as a short from 0-5
            var power = ContentHelpers.RoundToLevels((double) PowerLevel, component.PowerLevelMax, 8);

            // Set the alert level
            _alertsSystem.ShowAlert(uid, AlertType.ShadekinPower, (short) power);
        }


        /// <remarks> For viewing purposes. </remarks>
        /// <param name="PowerLevel">The current power level.</param>
        /// <returns>Power level as an integer.</returns>
        public int GetLevelInt(float PowerLevel)
        {
            // Very dumb, round and convert to int
            return (int) Math.Round(PowerLevel);
        }


        /// <summary>
        ///     Tries to update the power level of a shadekin based on an amount of seconds.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="frameTime">The time since the last update in seconds.</param>
        public bool TryUpdatePowerLevel(EntityUid uid, float frameTime)
        {
            // Check if the entity has a shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component)) return false;

            // Check if power gain is enabled
            if (!component.PowerLevelGainEnabled) return false;

            // Set the new power level
            UpdatePowerLevel(uid, frameTime);

            return true;
        }

        /// <summary>
        ///     Updates the power level of a shadekin based on an amount of seconds.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="frameTime">The time since the last update in seconds.</param>
        public void UpdatePowerLevel(EntityUid uid, float frameTime)
        {
            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to update power level of entity without shadekin component.");
                throw new InvalidOperationException("Tried to update power level of entity without shadekin component.");
            }

            // Calculate new power level (P = P + t * G * M)
            var newPowerLevel = component.PowerLevel + frameTime * component.PowerLevelGain * component.PowerLevelGainMultiplier;

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            SetPowerLevel(uid, newPowerLevel);
        }


        /// <summary>
        ///     Tries to add to the power level of a shadekin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="amount">The amount to add to the power level.</param>
        public bool TryAddPowerLevel(EntityUid uid, float amount)
        {
            // Check if the entity has a shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component)) return false;

            // Set the new power level
            AddPowerLevel(uid, amount);

            return true;
        }

        /// <summary>
        ///     Adds to the power level of a shadekin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="amount">The amount to add to the power level.</param>
        public void AddPowerLevel(EntityUid uid, float amount)
        {
            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to add to power level of entity without shadekin component.");
                throw new InvalidOperationException("Tried to add to power level of entity without shadekin component.");
            }

            // Get new power level
            var newPowerLevel = component.PowerLevel + amount;

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            SetPowerLevel(uid, newPowerLevel);
        }


        /// <summary>
        ///     Sets the power level of a shadekin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="newPowerLevel">The new power level.</param>
        public void SetPowerLevel(EntityUid uid, float newPowerLevel)
        {
            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to set power level of entity without shadekin component.");
                throw new InvalidOperationException("Tried to set power level of entity without shadekin component.");
            }

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            component._powerLevel = newPowerLevel;
        }


        /// <summary>
        ///     Tries to blackeye a shadekin.
        /// </summary>
        public bool TryBlackeye(EntityUid uid)
        {
            // Check if the entity has a shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component)) return false;

            if (!component.Blackeye &&
                component.PowerLevel <= ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min] + 1f)
            {
                Blackeye(uid);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Blackeyes a shadekin.
        /// </summary>
        public void Blackeye(EntityUid uid)
        {
            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to blackeye entity without shadekin component.");
                throw new InvalidOperationException("Tried to blackeye entity without shadekin component.");
            }

            component.Blackeye = true;
            RaiseNetworkEvent(new ShadekinBlackeyeEvent(component.Owner));
            RaiseLocalEvent(new ShadekinBlackeyeEvent(component.Owner));
        }


        /// <summary>
        ///     Tries to add a power multiplier.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="multiplier">The multiplier to add.</param>
        /// <param name="time">The time in seconds to wait before removing the multiplier.</param>
        public bool TryAddMultiplier(EntityUid uid, float multiplier = 1f, float? time = null)
        {
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var _)) return false;
            if (multiplier == float.NaN) return false;

            AddMultiplier(uid, multiplier, time);

            return true;
        }

        /// <summary>
        ///     Adds a power multiplier.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="multiplier">The multiplier to add.</param>
        /// <param name="time">The time in seconds to wait before removing the multiplier.</param>
        public void AddMultiplier(EntityUid uid, float multiplier = 1f, float? time = null)
        {
            // Get shadekin component
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                Logger.Error("Tried to add multiplier to entity without shadekin component.");
                throw new InvalidOperationException("Tried to add multiplier to entity without shadekin component.");
            }

            // Add the multiplier
            component.PowerLevelGainMultiplier += multiplier;

            // Remove the multiplier after a certain amount of time
            if (time != null)
            {
                Task.Run(async () =>
                {
                    await Task.Delay((int) time * 1000);
                    component.PowerLevelGainMultiplier -= multiplier;
                });
            }
        }
    }
}
