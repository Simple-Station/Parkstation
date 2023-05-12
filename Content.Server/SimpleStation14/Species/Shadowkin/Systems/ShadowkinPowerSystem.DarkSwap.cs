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
        ToggleInvisibility(args.Performer, args);

        args.Handled = true;
    }


    public void ToggleInvisibility(EntityUid uid, ShadowkinDarkSwapEvent args)
    {
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var comp))
            return;

        if (!HasComp<ShadowkinDarkSwappedComponent>(uid))
        {
            EnsureComp<ShadowkinDarkSwappedComponent>(uid);
            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, true));

            _audio.PlayPvs(args.SoundOn, args.Performer, AudioParams.Default.WithVolume(args.VolumeOn));

            _power.TryAddPowerLevel(comp.Owner, -args.PowerCostOn);
            _stamina.TakeStaminaDamage(comp.Owner, args.StaminaCost);
        }
        else
        {
            RemComp<ShadowkinDarkSwappedComponent>(uid);
            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, false));

            _audio.PlayPvs(args.SoundOff, args.Performer, AudioParams.Default.WithVolume(args.VolumeOff));

            _power.TryAddPowerLevel(comp.Owner, -args.PowerCostOff);
            _stamina.TakeStaminaDamage(comp.Owner, args.StaminaCost);
        }
    }


    private void OnInvisStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
    {
        EnsureComp<PacifiedComponent>(uid);

        SetCanSeeInvisibility(uid, true);
    }

    private void OnInvisShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
    {
        if (Terminating(uid))
            return;

        RemComp<PacifiedComponent>(uid);

        SetCanSeeInvisibility(uid, false);

        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var shadowkin))
            return;

        foreach (var light in shadowkin.DarkenedLights.ToArray())
        {
            if (!_entity.TryGetComponent<PointLightComponent>(light, out var pointLight) ||
                !_entity.TryGetComponent<ShadowkinLightComponent>(light, out var shadowkinLight))
                continue;

            _darken.ResetLight(pointLight, shadowkinLight);
        }

        shadowkin.DarkenedLights.Clear();
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
