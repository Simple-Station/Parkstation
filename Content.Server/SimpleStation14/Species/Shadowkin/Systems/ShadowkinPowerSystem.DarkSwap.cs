using Content.Server.Ghost.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Events;
using Content.Server.Visible;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinDarkSwapSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly ShadowkinDarkenSystem _darken = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private InstantAction _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        _action = new InstantAction(_prototype.Index<InstantActionPrototype>("ShadowkinDarkSwap"));

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentShutdown>(Shutdown);

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ShadowkinDarkSwapEvent>(DarkSwap);

        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);
    }


    private void Startup(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, _action, uid);
    }

    private void Shutdown(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, _action);
    }


    private void DarkSwap(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ShadowkinDarkSwapEvent args)
    {
        var hasComp = _entity.HasComponent<ShadowkinDarkSwappedComponent>(args.Performer);

        SetDarkened(
            args.Performer,
            !hasComp,
            !hasComp,
            true,
            args.StaminaCostOn,
            args.PowerCostOn,
            args.SoundOn,
            args.VolumeOn,
            args.StaminaCostOff,
            args.PowerCostOff,
            args.SoundOff,
            args.VolumeOff
        );

        args.Handled = true;
    }


    public void SetDarkened(
        EntityUid performer,
        bool addComp,
        bool invisible,
        bool darken,
        float staminaCostOn,
        float powerCostOn,
        SoundSpecifier soundOn,
        float volumeOn,
        float staminaCostOff,
        float powerCostOff,
        SoundSpecifier soundOff,
        float volumeOff
    )
    {
        var ev = new ShadowkinDarkSwapAttemptEvent();
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return;

        if (addComp)
        {
            var comp = _entity.EnsureComponent<ShadowkinDarkSwappedComponent>(performer);
            comp.Invisible = invisible;
            comp.Darken = darken;

            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(performer, true));

            _audio.PlayPvs(soundOn, performer, AudioParams.Default.WithVolume(volumeOn));

            _power.TryAddPowerLevel(performer, -powerCostOn);
            _stamina.TakeStaminaDamage(performer, staminaCostOn);
        }
        else
        {
            _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(performer);
            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(performer, false));

            _audio.PlayPvs(soundOff, performer, AudioParams.Default.WithVolume(volumeOff));

            _power.TryAddPowerLevel(performer, -powerCostOff);
            _stamina.TakeStaminaDamage(performer, staminaCostOff);
        }
    }


    private void OnInvisStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
    {
        EnsureComp<PacifiedComponent>(uid);

        if (component.Invisible)
            SetCanSeeInvisibility(uid, true);
    }

    private void OnInvisShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
    {
        RemComp<PacifiedComponent>(uid);

        if (component.Invisible)
            SetCanSeeInvisibility(uid, false);

        component.Darken = false;

        foreach (var light in component.DarkenedLights.ToArray())
        {
            if (!_entity.TryGetComponent<PointLightComponent>(light, out var pointLight) ||
                !_entity.TryGetComponent<ShadowkinLightComponent>(light, out var shadowkinLight))
                continue;

            _darken.ResetLight(pointLight, shadowkinLight);
        }

        component.DarkenedLights.Clear();
    }


    public void SetCanSeeInvisibility(EntityUid uid, bool set)
    {
        var visibility = _entity.EnsureComponent<VisibilityComponent>(uid);

        if (set)
        {
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
            {
                eye.VisibilityMask |= (uint) VisibilityFlags.DarkSwapInvisibility;
            }

            _visibility.AddLayer(uid, visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
            _visibility.RemoveLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid);

            if (!_entity.TryGetComponent<GhostComponent>(uid, out var _))
                _stealth.SetVisibility(uid, 0.8f, _entity.EnsureComponent<StealthComponent>(uid));
        }
        else
        {
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
            {
                eye.VisibilityMask &= ~(uint) VisibilityFlags.DarkSwapInvisibility;
            }

            _visibility.RemoveLayer(uid, visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
            _visibility.AddLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid);

            if (!_entity.TryGetComponent<GhostComponent>(uid, out var _))
                _entity.RemoveComponent<StealthComponent>(uid);
        }
    }
}
