using Content.Server.Cloning;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Server.SimpleStation14.Traits.Events;
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

        SubscribeLocalEvent<ShadowkinBlackeyeAttemptEvent>(OnBlackeyeAttempt);
        SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);

        SubscribeLocalEvent<BeenClonedEvent>(OnCloned);
    }


    private void OnBlackeyeAttempt(ShadowkinBlackeyeAttemptEvent ev)
    {
        // Cancel if one of the following is true:
        // - The entity is not a shadowkin
        // - The entity is already blackeyed
        // - The entity has more than 5 power and ev.CheckPower is true
        if (!_entity.TryGetComponent<ShadowkinComponent>(ev.Uid, out var component) ||
            component.Blackeye ||
            (ev.CheckPower && component.PowerLevel > ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min] + 5))
            ev.Cancel();
    }

    private void OnBlackeye(ShadowkinBlackeyeEvent ev)
    {
        // Check if the entity is a shadowkin
        if (!_entity.TryGetComponent<ShadowkinComponent>(ev.Uid, out var component))
            return;

        // Stop gaining power
        component.Blackeye = true;
        component.PowerLevelGainEnabled = false;
        _power.SetPowerLevel(ev.Uid, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);

        // Update client state
        Dirty(component);

        // Remove powers
        _entity.RemoveComponent<ShadowkinDarkSwapPowerComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinRestPowerComponent>(ev.Uid);
        _entity.RemoveComponent<ShadowkinTeleportPowerComponent>(ev.Uid);
        _entity.RemoveComponent<EmpathyChatComponent>(ev.Uid);

        if (!ev.Damage)
            return;

        // Popup
        _popup.PopupEntity(Loc.GetString("shadowkin-blackeye"), ev.Uid, ev.Uid, PopupType.Large);

        // Stamina crit
        if (_entity.TryGetComponent<StaminaComponent>(ev.Uid, out var stamina))
        {
            _stamina.TakeStaminaDamage(ev.Uid, stamina.CritThreshold, null, ev.Uid);
        }

        // Nearly crit with cellular damage
        // If already 5 damage off of crit, don't do anything
        if (!_entity.TryGetComponent<DamageableComponent>(ev.Uid, out var damageable) ||
            !_mobThreshold.TryGetThresholdForState(ev.Uid, MobState.Critical, out var key))
            return;

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


    private void OnCloned(BeenClonedEvent ev)
    {
        // Don't give blackeyed Shadowkin their abilities back when they're cloned.
        if (_entity.TryGetComponent<ShadowkinComponent>(ev.OriginalMob, out var shadowkin) &&
            shadowkin.Blackeye)
            _power.TryBlackeye(ev.Mob, false, false);

        // Blackeye the Shadowkin that come from the metempsychosis machine
        if (_entity.HasComponent<MetempsychoticMachineComponent>(ev.Cloner) &&
            _entity.HasComponent<ShadowkinComponent>(ev.Mob))
            _power.TryBlackeye(ev.Mob, false, false);
    }
}
