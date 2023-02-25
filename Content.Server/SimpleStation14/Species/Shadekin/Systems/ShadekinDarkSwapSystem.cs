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
                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(uid, true));

                _staminaSystem.TakeStaminaDamage(uid, args.StaminaCostOn);
            }
            else
            {
                RemComp<ShadekinDarkSwappedComponent>(uid);
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

            var visibility = EntityManager.EnsureComponent<VisibilityComponent>(uid);

            _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
            _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(visibility);

            SetCanSeeInvisibility(uid, true);
        }

        private void OnInvisShutdown(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid)) return;

            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<PacifiedComponent>(uid);
            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            Dirty(uid);

            //////////////////////////////////////////

            if (TryComp<VisibilityComponent>(uid, out var visibility))
            {
                _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.DarkSwapInvisibility, false);
                _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(visibility);
            }

            SetCanSeeInvisibility(uid, false);
        }

        public void SetCanSeeInvisibility(EntityUid uid, bool set)
        {
            if (set == true)
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask |= (uint) VisibilityFlags.DarkSwapInvisibility;
                }
            }
            else
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    eye.VisibilityMask &= ~(uint) VisibilityFlags.DarkSwapInvisibility;
                }
            }
        }

        private void OnEntInserted(EntityUid uid, ShadekinDarkSwappedComponent component, EntInsertedIntoContainerMessage args)
        {
            Dirty(args.Entity);
        }

        private void OnEntRemoved(EntityUid uid, ShadekinDarkSwappedComponent component, EntRemovedFromContainerMessage args)
        {
            Dirty(args.Entity);
        }
    }
}
