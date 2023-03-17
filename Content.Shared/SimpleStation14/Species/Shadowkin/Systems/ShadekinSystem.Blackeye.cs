using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
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

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinBlackeyeSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
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

            SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadowkinBlackeyeEvent ev)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadowkinDarkSwapPowerComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadowkinDarkSwappedComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadowkinRestPowerComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadowkinTeleportPowerComponent>(ev.Euid);

            // Popup
            if (_net.IsClient)
            {
                _popupSystem.PopupEntity(Loc.GetString("shadowkin-blackeye"), ev.Euid, ev.Euid, PopupType.Large);
            }

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadowkinComponent>(ev.Euid, out var component))
            {
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component.Owner, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);
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
                    new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Cellular"), key.Value - 10),
                    true,
                    true,
                    null,
                    null,
                    false
                );
            }
        }
    }
}
