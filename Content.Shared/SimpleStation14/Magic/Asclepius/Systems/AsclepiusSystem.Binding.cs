using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly SharedItemSystem _itemSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AsclepiusStaffComponent, ComponentGetState>(GetCompState);
            SubscribeLocalEvent<AsclepiusStaffComponent, ComponentHandleState>(HandleCompState);

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


            // Bind the staff to the user
            component.BoundTo = args.User;
            _entityManager.AddComponent<HippocraticOathComponent>(args.User);

            // Set description
            var meta = _entityManager.GetComponent<MetaDataComponent>(args.Staff);
            meta.EntityDescription = Loc.GetString("asclepius-staff-description-bound");

            // Set the inhand sprite prefix
            _itemSystem.SetHeldPrefix(args.Staff, "active");


            // Tell the user
            if (_net.IsServer) _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-hippocratic-oath-complete"), args.User, args.User, PopupType.MediumCaution);

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

            // Tell the user
            if (_net.IsServer) _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-hippocratic-oath-cancelled"), args.User, args.User, PopupType.Medium);

            Dirty(args.Staff);
        }
    }
}
