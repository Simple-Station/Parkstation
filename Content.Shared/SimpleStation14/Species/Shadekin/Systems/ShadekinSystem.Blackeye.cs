using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Shared.Mobs.Systems;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystemBlackeyeSystem : EntitySystem
    {
        ShadekinSystemPowerSystem _powerSystem = new();
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadekinBlackeyeEvent ev)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadekinDarkSwapComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinTeleportComponent>(ev.Euid);

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadekinComponent>(ev.Euid, out var component))
            {
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min]);
            }

            // Stamina crit
            if (_entityManager.TryGetComponent<StaminaComponent>(ev.Euid, out var staminaComponent))
            {
                _staminaSystem.TakeStaminaDamage(ev.Euid, 100, null, ev.Euid);
            }

            // Take enough damage to be just barely out of critical
            if (_entityManager.TryGetComponent<DamageableComponent>(ev.Euid, out var damageable) &&
                _mobThresholdSystem.TryGetThresholdForState(ev.Euid, MobState.Critical, out var key))
            {
                _damageableSystem.SetAllDamage(damageable, 0);

                _damageableSystem.TryChangeDamage(
                    ev.Euid,
                    new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Cellular"), key.Value - 1),
                    true
                );
            }
        }
    }
}
