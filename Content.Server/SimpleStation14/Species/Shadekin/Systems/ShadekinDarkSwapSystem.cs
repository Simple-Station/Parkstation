using Content.Server.Ghost.Components;
using Content.Server.Psionics;
using Content.Server.Visible;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinDarkSwapSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ShadekinDarkenSystem _darkenSystem = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly SharedStealthSystem _stealthSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinDarkSwapPowerComponent, ShadekinDarkSwapEvent>(DarkSwap);

            SubscribeLocalEvent<EyeComponent, ComponentStartup>(OnEyeStartup);

            SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
            SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);
        }


        private void DarkSwap(EntityUid uid, ShadekinDarkSwapPowerComponent component, ShadekinDarkSwapEvent args)
        {
            ToggleInvisibility(args.Performer, args);

            args.Handled = true;
        }


        public void ToggleInvisibility(EntityUid uid, ShadekinDarkSwapEvent args)
        {
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var comp)) return;

            if (!HasComp<ShadekinDarkSwappedComponent>(uid))
            {
                EnsureComp<ShadekinDarkSwappedComponent>(uid);
                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(uid, true));

                _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCostOn);
                _staminaSystem.TakeStaminaDamage(comp.Owner, args.StaminaCost);
            }
            else
            {
                RemComp<ShadekinDarkSwappedComponent>(uid);
                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(uid, false));

                _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCostOff);
                _staminaSystem.TakeStaminaDamage(comp.Owner, args.StaminaCost);
            }
        }


        private void OnEyeStartup(EntityUid uid, EyeComponent component, ComponentStartup args)
        {
            if (!_entityManager.TryGetComponent<GhostComponent>(uid, out var _))
                SetCanSeeInvisibility(uid, false);
            else
                SetCanSeeInvisibility(uid, true);
        }

        private void OnInvisStartup(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentStartup args)
        {
            EnsureComp<PacifiedComponent>(uid);

            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            SetCanSeeInvisibility(uid, true);
        }

        private void OnInvisShutdown(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid)) return;

            RemComp<PacifiedComponent>(uid);
            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            SetCanSeeInvisibility(uid, false);

            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var shadekin)) return;

            foreach (var light in shadekin.DarkenedLights.ToArray())
            {
                if (!_entityManager.TryGetComponent<PointLightComponent>(light, out var pointLight) ||
                    !_entityManager.TryGetComponent<ShadekinLightComponent>(light, out var shadekinLight))
                    continue;

                _darkenSystem.ResetLight(pointLight, shadekinLight);
            }

            shadekin.DarkenedLights.Clear();
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
