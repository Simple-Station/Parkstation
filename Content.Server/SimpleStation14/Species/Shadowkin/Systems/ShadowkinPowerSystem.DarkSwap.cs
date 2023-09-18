using Content.Server.Ghost.Components;
using Content.Server.Magic;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Events;
using Content.Server.Visible;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Cuffs.Components;
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
    [Dependency] private readonly MagicSystem _magic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentShutdown>(Shutdown);

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ShadowkinDarkSwapEvent>(DarkSwap);

        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);
    }


    private void Startup(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, new InstantAction(_prototype.Index<InstantActionPrototype>("ShadowkinDarkSwap")), null);
    }

    private void Shutdown(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, new InstantAction(_prototype.Index<InstantActionPrototype>("ShadowkinDarkSwap")));
    }


    private void DarkSwap(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ShadowkinDarkSwapEvent args)
    {
        // Need power to drain power
        if (!_entity.HasComponent<ShadowkinComponent>(args.Performer))
            return;

        // Don't activate abilities if specially handcuffed
        if (_entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) && cuffs.AntiShadowkin)
            return;


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
            args.VolumeOff,
            args
        );

        _magic.Speak(args, false);
    }


    /// <summary>
    ///     Handles the effects of darkswapping
    /// </summary>
    /// <param name="performer">The entity being modified</param>
    /// <param name="addComp">Is the entity swapping in to or out of The Dark?</param>
    /// <param name="invisible">Should the entity become invisible?</param>
    /// <param name="darken">Should darkswapping darken nearby lights?</param>
    /// <param name="staminaCostOn">Stamina cost for darkswapping</param>
    /// <param name="powerCostOn">Power cost for darkswapping</param>
    /// <param name="soundOn">Sound for the darkswapping</param>
    /// <param name="volumeOn">Volume for the on sound</param>
    /// <param name="staminaCostOff">Stamina cost for un swapping</param>
    /// <param name="powerCostOff">Power cost for un swapping</param>
    /// <param name="soundOff">Sound for the un swapping</param>
    /// <param name="volumeOff">Volume for the off sound</param>
    /// <param name="args">If from an event, handle it</param>
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
        float volumeOff,
        ShadowkinDarkSwapEvent? args
    )
    {
        var ev = new ShadowkinDarkSwapAttemptEvent(performer);
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

        if (args != null)
            args.Handled = true;
    }


    private void OnInvisStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
    {
        EnsureComp<PacifiedComponent>(uid);

        if (component.Invisible)
            SetCanSeeInvisibility(uid, true, true, true);
    }

    private void OnInvisShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
    {
        RemComp<PacifiedComponent>(uid);

        if (component.Invisible)
            SetCanSeeInvisibility(uid, false, true, true);

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


    /// <summary>
    ///     Makes the specified entity able to see Shadowkin invisibility.
    /// </summary>
    /// <param name="uid">Entity to modify</param>
    /// <param name="enabled">Whether the entity can see invisibility</param>
    /// <param name="invisibility">Should the entity be moved to another visibility layer?</param>
    /// <param name="stealth">(Only gets considered if set is true) Adds stealth to the entity</param>
    public void SetCanSeeInvisibility(EntityUid uid, bool enabled, bool invisibility, bool stealth)
    {
        var visibility = _entity.EnsureComponent<VisibilityComponent>(uid);

        if (enabled)
        {
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
            {
                eye.VisibilityMask |= (uint) VisibilityFlags.DarkSwapInvisibility;
            }

            if (invisibility)
            {
                _visibility.AddLayer(uid, visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibility.RemoveLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            }
            _visibility.RefreshVisibility(uid);

            if (stealth)
                _stealth.SetVisibility(uid, 0.8f, _entity.EnsureComponent<StealthComponent>(uid));
        }
        else
        {
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
            {
                eye.VisibilityMask &= ~(uint) VisibilityFlags.DarkSwapInvisibility;
            }

            if (invisibility)
            {
                _visibility.RemoveLayer(uid, visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibility.AddLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            }
            _visibility.RefreshVisibility(uid);

            _entity.RemoveComponent<StealthComponent>(uid);
        }
    }
}
