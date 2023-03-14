using System.Threading;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AscleipusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AsclepiusStaffComponent, UseInHandEvent>(OnUseInHand);
        }

        private void OnUseInHand(EntityUid uid, AsclepiusStaffComponent component, UseInHandEvent args)
        {
            // Already bound, ignore
            // TODO: Do something to the user?
            if (component.BoundTo != EntityUid.Invalid)
            {
                return;
            }


            // Start the oath
            _chatSystem.TrySendInGameICMessage(args.User, Loc.GetString("asclepius-binding-hippocratic-oath-start"), InGameICChatType.Speak, false);

            component.CancelToken = new CancellationTokenSource();
            _doAfter.DoAfter(new DoAfterEventArgs(args.User, 5f, component.CancelToken.Token)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                BreakOnStun = true,
                MovementThreshold = 0.25f,
                BroadcastFinishedEvent = new HippocraticOathCompleteEvent()
                {
                    Staff = component.Owner,
                    User = args.User,
                },
                BroadcastCancelledEvent = new HippocraticOathCancelledEvent()
                {
                    Staff = component.Owner,
                    User = args.User,
                }
            });
        }
    }
}
