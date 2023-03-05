using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Network;

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
        [Dependency] private readonly SharedHumanoidAppearanceSystem _sharedHumanoidAppearance = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeAllEvent<ShadekinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadekinBlackeyeEvent ev)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadekinDarkSwapComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinTeleportComponent>(ev.Euid);

            // Popup
            if (_net.IsClient)
            {
                _popupSystem.PopupEntity(Loc.GetString("shadekin-blackeye"), ev.Euid, ev.Euid, PopupType.Large);
            }

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadekinComponent>(ev.Euid, out var component))
            {
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component.Owner, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min]);
            }

            // Stamina crit
            if (_entityManager.TryGetComponent<StaminaComponent>(ev.Euid, out var staminaComponent))
            {
                _staminaSystem.TakeStaminaDamage(ev.Euid, 100, null, ev.Euid);
            }

            // Near damage crit
            if (_entityManager.TryGetComponent<DamageableComponent>(ev.Euid, out var damageable) &&
                _mobThresholdSystem.TryGetThresholdForState(ev.Euid, MobState.Critical, out var key))
            {
                // I am aware this removes all other damage, though I am adding cellular in place of it
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
