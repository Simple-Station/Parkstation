using Content.Shared.Alert;
using Content.Shared.Rounding;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using System.Threading.Tasks;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinPowerSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;


        /// <param name="powerLevel">The current power level.</param>
        /// <returns>The name of the power level.</returns>
        public string GetLevelName(float powerLevel)
        {
            // Placeholders
            var result = ShadowkinPowerThreshold.Min;
            var value = ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Max];

            // Find the highest threshold that is lower than the current power level
            foreach (var threshold in ShadowkinComponent.PowerThresholds)
            {
                if (threshold.Value <= value && threshold.Value >= powerLevel)
                {
                    result = threshold.Key;
                    value = threshold.Value;
                }
            }

            var powerDictionary = new Dictionary<ShadowkinPowerThreshold, string>
            {
                {ShadowkinPowerThreshold.Max, Loc.GetString("shadowkin-power-max")},
                {ShadowkinPowerThreshold.Great, Loc.GetString("shadowkin-power-great")},
                {ShadowkinPowerThreshold.Good, Loc.GetString("shadowkin-power-good")},
                {ShadowkinPowerThreshold.Okay, Loc.GetString("shadowkin-power-okay")},
                {ShadowkinPowerThreshold.Tired, Loc.GetString("shadowkin-power-tired")},
                {ShadowkinPowerThreshold.Min, Loc.GetString("shadowkin-power-min")}
            };

            // Return the name of the threshold
            powerDictionary.TryGetValue(result, out var powerType);
            return powerType ?? Loc.GetString("shadowkin-power-okay");
        }

        /// <summary>
        ///    Sets the alert level of a shadowkin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="enabled">Enable the alert or not</param>
        /// <param name="powerLevel">The current power level.</param>
        public void UpdateAlert(EntityUid uid, bool enabled, float? powerLevel = null)
        {
            if (!enabled || powerLevel == null)
            {
                _alertsSystem.ClearAlert(uid, AlertType.ShadowkinPower);
                return;
            }

            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to update alert of entity without shadowkin component.");
                throw new InvalidOperationException("Tried to update alert of entity without shadowkin component.");
            }

            // Get the power as a short from 0-5
            var power = ContentHelpers.RoundToLevels((double) powerLevel, component.PowerLevelMax, 8);

            // Set the alert level
            _alertsSystem.ShowAlert(uid, AlertType.ShadowkinPower, (short) power);
        }


        /// <summary>
        ///     Tries to update the power level of a shadowkin based on an amount of seconds.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="frameTime">The time since the last update in seconds.</param>
        public bool TryUpdatePowerLevel(EntityUid uid, float frameTime)
        {
            // Check if the entity has a shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
                return false;

            // Check if power gain is enabled
            if (!component.PowerLevelGainEnabled)
                return false;

            // Set the new power level
            UpdatePowerLevel(uid, frameTime);

            return true;
        }

        /// <summary>
        ///     Updates the power level of a shadowkin based on an amount of seconds.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="frameTime">The time since the last update in seconds.</param>
        public void UpdatePowerLevel(EntityUid uid, float frameTime)
        {
            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to update power level of entity without shadowkin component.");
                throw new InvalidOperationException("Tried to update power level of entity without shadowkin component.");
            }

            // Calculate new power level (P = P + t * G * M)
            var newPowerLevel = component.PowerLevel + frameTime * component.PowerLevelGain * component.PowerLevelGainMultiplier;

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            SetPowerLevel(uid, newPowerLevel);
        }


        /// <summary>
        ///     Tries to add to the power level of a shadowkin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="amount">The amount to add to the power level.</param>
        public bool TryAddPowerLevel(EntityUid uid, float amount)
        {
            // Check if the entity has a shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out _))
                return false;

            // Set the new power level
            AddPowerLevel(uid, amount);

            return true;
        }

        /// <summary>
        ///     Adds to the power level of a shadowkin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="amount">The amount to add to the power level.</param>
        public void AddPowerLevel(EntityUid uid, float amount)
        {
            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to add to power level of entity without shadowkin component.");
                throw new InvalidOperationException("Tried to add to power level of entity without shadowkin component.");
            }

            // Get new power level
            var newPowerLevel = component.PowerLevel + amount;

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            SetPowerLevel(uid, newPowerLevel);
        }


        /// <summary>
        ///     Sets the power level of a shadowkin.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="newPowerLevel">The new power level.</param>
        public void SetPowerLevel(EntityUid uid, float newPowerLevel)
        {
            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to set power level of entity without shadowkin component.");
                throw new InvalidOperationException("Tried to set power level of entity without shadowkin component.");
            }

            // Clamp power level using clamp function
            newPowerLevel = Math.Clamp(newPowerLevel, component.PowerLevelMin, component.PowerLevelMax);

            // Set the new power level
            component._powerLevel = newPowerLevel;
        }


        /// <summary>
        ///     Tries to blackeye a shadowkin.
        /// </summary>
        public bool TryBlackeye(EntityUid uid)
        {
            // Check if the entity has a shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
                return false;

            if (!component.Blackeye &&
                component.PowerLevel <= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min])
            {
                Blackeye(uid);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Blackeyes a shadowkin.
        /// </summary>
        public void Blackeye(EntityUid uid)
        {
            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to blackeye entity without shadowkin component.");
                throw new InvalidOperationException("Tried to blackeye entity without shadowkin component.");
            }

            component.Blackeye = true;
            RaiseNetworkEvent(new ShadowkinBlackeyeEvent(component.Owner));
            RaiseLocalEvent(new ShadowkinBlackeyeEvent(component.Owner));
        }


        /// <summary>
        ///     Tries to add a power multiplier.
        /// </summary>
        /// <param name="uid">The entity uid.</param>
        /// <param name="multiplier">The multiplier to add.</param>
        /// <param name="time">The time in seconds to wait before removing the multiplier.</param>
        public bool TryAddMultiplier(EntityUid uid, float multiplier = 1f, float? time = null)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var _))
                return false;
            if (float.IsNaN(multiplier))
                return false;

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
            // Get shadowkin component
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                Logger.Error("Tried to add multiplier to entity without shadowkin component.");
                throw new InvalidOperationException("Tried to add multiplier to entity without shadowkin component.");
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
