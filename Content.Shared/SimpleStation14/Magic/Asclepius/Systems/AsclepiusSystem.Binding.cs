using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AsclepiusStaffComponent, ComponentGetState>(GetCompState);
            SubscribeLocalEvent<AsclepiusStaffComponent, ComponentHandleState>(HandleCompState);

            // Move these to AsclepiusSystem.Staff
            SubscribeLocalEvent<AsclepiusStaffComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
            SubscribeLocalEvent<AsclepiusStaffComponent, DroppedEvent>(OnDropped);

            SubscribeAllEvent<HippocraticOathCompleteEvent>(OnHippocraticOathComplete);
            SubscribeAllEvent<HippocraticOathCancelledEvent>(OnHippocraticOathCancelled);
        }

        private void GetCompState(EntityUid uid, AsclepiusStaffComponent component, ref ComponentGetState args)
        {
            args.State = new AsclepiusStaffComponentState
            {
                BoundTo = component.BoundTo,
                PacifyBound = component.PacifyBound,
                RegenerateOnRemoval = component.RegenerateOnRemoval,
                PermanentOath = component.PermanentOath,
            };
        }

        private void HandleCompState(EntityUid uid, AsclepiusStaffComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not AsclepiusStaffComponentState state) return;

            component.BoundTo = state.BoundTo;
            component.PacifyBound = state.PacifyBound;
            component.RegenerateOnRemoval = state.RegenerateOnRemoval;
            component.PermanentOath = state.PermanentOath;
        }


        private void OnRemoveAttempt(EntityUid uid, AsclepiusStaffComponent component, ContainerGettingRemovedAttemptEvent args)
        {
            // Cancel the oath if it's in progress
            if (component.CancelToken != null)
            {
                component.CancelToken.Cancel();
                component.CancelToken = null;
            }

            // Don't allow dropping the staff if bound
            if (component.BoundTo != EntityUid.Invalid) args.Cancel();
        }

        private void OnDropped(EntityUid uid, AsclepiusStaffComponent component, DroppedEvent args)
        {
            // Cancel the oath if it's in progress
            if (component.CancelToken != null)
            {
                component.CancelToken.Cancel();
                component.CancelToken = null;
            }
        }


        private void OnHippocraticOathComplete(HippocraticOathCompleteEvent args)
        {
            if (args.Cancelled) return;

            // How did you do the oath?
            if (!_entityManager.TryGetComponent<AsclepiusStaffComponent>(args.Staff, out var component))
            {
                return;
            }

            // Tell the client
            if (_net.IsServer) RaiseNetworkEvent(new HippocraticOathCompleteEvent()
            {
                Staff = args.Staff,
                User = args.User,
            });

            // Clear the cancel token
            component.CancelToken = null;
            // Bind the staff to the user
            component.BoundTo = args.User;

            // Tell the user
            _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-hippocratic-oath-complete"), args.User, PopupType.MediumCaution);

            Dirty(args.Staff);
        }

        private void OnHippocraticOathCancelled(HippocraticOathCancelledEvent args)
        {
            if (args.Cancelled) return;

            // How did you do the oath?
            if (!_entityManager.TryGetComponent<AsclepiusStaffComponent>(args.Staff, out var component))
            {
                return;
            }

            // Tell the client
            if (_net.IsServer) RaiseNetworkEvent(new HippocraticOathCancelledEvent()
            {
                Staff = args.Staff,
                User = args.User,
            });

            // Clear the cancel token
            component.CancelToken?.Cancel();
            component.CancelToken = null;

            // Tell the user
            _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-hippocratic-oath-cancelled"), args.User, PopupType.Medium);
        }
    }
}
