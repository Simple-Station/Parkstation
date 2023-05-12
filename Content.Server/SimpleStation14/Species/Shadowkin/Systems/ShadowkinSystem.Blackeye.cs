using Content.Server.SimpleStation14.Species.Shadowkin.Components;
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

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinBlackeyeSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
    }

    private void OnBlackeye(ShadowkinBlackeyeEvent ev)
    {
        // Popup
        _popup.PopupEntity(Loc.GetString("shadowkin-blackeye"), ev.Uid, ev.Uid, PopupType.Large);

        // Stop gaining power
        if (_entity.TryGetComponent<ShadowkinComponent>(ev.Uid, out var component))
        {
            component.Blackeye = true;
            component.PowerLevelGainEnabled = false;
            _power.SetPowerLevel(ev.Uid, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);

            Dirty(component);
        }

        // Remove powers
        _entity.RemoveComponent<ShadowkinDarkSwapPowerComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinRestPowerComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinTeleportPowerComponent>(ev.Uid);

        if (!ev.Damage)
            return;

        // Stamina crit
        if (_entity.TryGetComponent<StaminaComponent>(ev.Uid, out var stamina))
        {
            _stamina.TakeStaminaDamage(ev.Uid, stamina.CritThreshold, null, ev.Uid);
        }

        // Nearly crit with cellular damage
        // If already 5 damage off of crit, don't do anything
        if (_entity.TryGetComponent<DamageableComponent>(ev.Uid, out var damageable) &&
            _mobThreshold.TryGetThresholdForState(ev.Uid, MobState.Critical, out var key))
        {
            var minus = damageable.TotalDamage;

            _damageable.TryChangeDamage(
                ev.Uid,
                new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Cellular"),
                    Math.Max((double) (key.Value - minus - 5), 0)),
                true,
                true,
                null,
                null,
                false
            );
        }
    }
}
