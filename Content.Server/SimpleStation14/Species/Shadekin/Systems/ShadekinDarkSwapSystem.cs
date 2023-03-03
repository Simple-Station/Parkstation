using Content.Server.Psionics;
using Content.Server.Visible;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinDarkSwapSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ShadekinDarkenSystem _darkenSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinDarkSwapComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinDarkSwapComponent, ComponentShutdown>(Shutdown);

            SubscribeLocalEvent<ShadekinDarkSwapComponent, ShadekinDarkSwapEvent>(DarkSwap);

            //////////////////////////////////////////

            /// Masking
            SubscribeLocalEvent<EyeComponent, ComponentStartup>(OnEyeStartup);

            /// Layer
            SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
            SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);

            // PVS Stuff
            SubscribeLocalEvent<ShadekinDarkSwappedComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
            SubscribeLocalEvent<ShadekinDarkSwappedComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        }

        private void Startup(EntityUid uid, ShadekinDarkSwapComponent component, ComponentStartup args)
        {
            var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("ShadekinDarkSwap"));
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadekinDarkSwapComponent component, ComponentShutdown args)
        {
            var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("ShadekinDarkSwap"));
            _actionsSystem.RemoveAction(uid, action);
        }


        private void DarkSwap(EntityUid uid, ShadekinDarkSwapComponent component, ShadekinDarkSwapEvent args)
        {
            ToggleInvisibility(args.Performer, args);

            args.Handled = true;
        }


        public void ToggleInvisibility(EntityUid uid, ShadekinDarkSwapEvent args)
        {
            if (!HasComp<ShadekinDarkSwappedComponent>(uid))
            {
                EnsureComp<ShadekinDarkSwappedComponent>(uid);
                SetCanSeeInvisibility(uid, true);
                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(uid, true));

                _staminaSystem.TakeStaminaDamage(uid, args.StaminaCostOn);
            }
            else
            {
                RemComp<ShadekinDarkSwappedComponent>(uid);
                SetCanSeeInvisibility(uid, false);
                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(uid, false));

                _staminaSystem.TakeStaminaDamage(uid, args.StaminaCostOff);
            }
        }

        //////////////////////////////////////////

        private void OnEyeStartup(EntityUid uid, EyeComponent component, ComponentStartup args)
        {
            SetCanSeeInvisibility(uid, false);
        }

        private void OnInvisStartup(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentStartup args)
        {
            EnsureComp<PsionicallyInvisibleComponent>(uid);
            EnsureComp<PacifiedComponent>(uid);

            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            //////////////////////////////////////////

            SetCanSeeInvisibility(uid, true);
        }

        private void OnInvisShutdown(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid)) return;

            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<PacifiedComponent>(uid);
            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            //////////////////////////////////////////

            SetCanSeeInvisibility(uid, false);

            //////////////////////////////////////////

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

        private void OnEntInserted(EntityUid uid, ShadekinDarkSwappedComponent component, EntInsertedIntoContainerMessage args)
        {
            Dirty(args.Entity);
        }

        private void OnEntRemoved(EntityUid uid, ShadekinDarkSwappedComponent component, EntRemovedFromContainerMessage args)
        {
            Dirty(args.Entity);
        }


        public void SetCanSeeInvisibility(EntityUid uid, bool set)
        {
            var visibility = EntityManager.EnsureComponent<VisibilityComponent>(uid);

            if (set == true)
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask |= (uint) VisibilityFlags.DarkSwapInvisibility;
                }

                _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(visibility);
            }
            else
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask &= ~(uint) VisibilityFlags.DarkSwapInvisibility;
                }

                _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(visibility);
            }
        }
    }
}
