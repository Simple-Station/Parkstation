using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinBlackeyeSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadowkinBlackeyeEvent ev)
        {
            // Popup
            _popupSystem.PopupEntity(Loc.GetString("shadowkin-blackeye"), ev.Uid, ev.Uid, PopupType.Large);

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadowkinComponent>(ev.Uid, out var component))
            {
                component.Blackeye = true;
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component.Owner, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);
            }


            if (!ev.Damage) return;

            // Stamina crit
            if (_entityManager.TryGetComponent<StaminaComponent>(ev.Uid, out var staminaComponent))
            {
                _staminaSystem.TakeStaminaDamage(ev.Uid, 100, null, ev.Uid);
            }

            // Near damage crit
            if (_entityManager.TryGetComponent<DamageableComponent>(ev.Uid, out var damageable) &&
                _mobThresholdSystem.TryGetThresholdForState(ev.Uid, MobState.Critical, out var key))
            {
                // I am aware this removes all other damage, though I am adding cellular in place of it
                _damageableSystem.SetAllDamage(damageable, 0);

                _damageableSystem.TryChangeDamage(
                    ev.Uid,
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
