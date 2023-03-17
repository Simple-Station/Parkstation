using Content.Server.Ghost.Components;
using Content.Server.Psionics;
using Content.Server.Visible;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadowkinDarkSwapSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ShadowkinDarkenSystem _darkenSystem = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly SharedStealthSystem _stealthSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public SharedAudioSystem Audio => _audio;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ShadowkinDarkSwapEvent>(DarkSwap);

            SubscribeLocalEvent<EyeComponent, ComponentStartup>(OnEyeStartup);

            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);
        }


        private void DarkSwap(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ShadowkinDarkSwapEvent args)
        {
            ToggleInvisibility(args.Performer, args);

            args.Handled = true;
        }


        public void ToggleInvisibility(EntityUid uid, ShadowkinDarkSwapEvent args)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var comp)) return;

            if (!HasComp<ShadowkinDarkSwappedComponent>(uid))
            {
                EnsureComp<ShadowkinDarkSwappedComponent>(uid);
                RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, true));

                _audio.PlayPvs(args.SoundOn, args.Performer, AudioParams.Default.WithVolume(args.VolumeOn));

                _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCostOn);
                _staminaSystem.TakeStaminaDamage(comp.Owner, args.StaminaCost);
            }
            else
            {
                RemComp<ShadowkinDarkSwappedComponent>(uid);
                RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, false));

                _audio.PlayPvs(args.SoundOff, args.Performer, AudioParams.Default.WithVolume(args.VolumeOff));

                _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCostOff);
                _staminaSystem.TakeStaminaDamage(comp.Owner, args.StaminaCost);
            }
        }


        private void OnEyeStartup(EntityUid uid, EyeComponent component, ComponentStartup args)
        {
            if (!_entityManager.TryGetComponent<GhostComponent>(uid, out var _))
            {
                SetCanSeeInvisibility(uid, false);
            }
            else
            {
                SetCanSeeInvisibility(uid, true);
            }
        }

        private void OnInvisStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
        {
            EnsureComp<PacifiedComponent>(uid);

            SetCanSeeInvisibility(uid, true);
        }

        private void OnInvisShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid)) return;

            RemComp<PacifiedComponent>(uid);

            SetCanSeeInvisibility(uid, false);

            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var shadowkin)) return;

            foreach (var light in shadowkin.DarkenedLights.ToArray())
            {
                if (!_entityManager.TryGetComponent<PointLightComponent>(light, out var pointLight) ||
                    !_entityManager.TryGetComponent<ShadowkinLightComponent>(light, out var shadowkinLight))
                    continue;

                _darkenSystem.ResetLight(pointLight, shadowkinLight);
            }

            shadowkin.DarkenedLights.Clear();
        }


        public void SetCanSeeInvisibility(EntityUid uid, bool set)
        {
            var visibility = _entityManager.EnsureComponent<VisibilityComponent>(uid);

            if (set == true)
            {
                if (_entityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask |= (uint) VisibilityFlags.DarkSwapInvisibility;
                }

                _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(visibility);

                if (!_entityManager.TryGetComponent<GhostComponent>(uid, out var _)) _stealthSystem.SetVisibility(uid, 0.8f, _entityManager.EnsureComponent<StealthComponent>(uid));
            }
            else
            {
                if (_entityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask &= ~(uint) VisibilityFlags.DarkSwapInvisibility;
                }

                _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(visibility);

                if (!_entityManager.TryGetComponent<GhostComponent>(uid, out var _)) _entityManager.RemoveComponent<StealthComponent>(uid);
            }
        }
    }
}
