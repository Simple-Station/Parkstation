using System.Threading;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AsclepiusStaffComponent, UseInHandEvent>(OnUseInHand);
        }

        private async void OnUseInHand(EntityUid uid, AsclepiusStaffComponent component, UseInHandEvent args)
        {
            // Already bound, ignore
            // TODO: Do something to the user?
            if (component.BoundTo != EntityUid.Invalid)
            {
                return;
            }


            Progress(uid, args.User, 0);
        }

        private async void Progress(EntityUid staff, EntityUid user, int progress)
        {
            // Why did you break the staff?
            if (!_entityManager.TryGetComponent<AsclepiusStaffComponent>(staff, out var component))
            {
                RaiseLocalEvent<HippocraticOathCancelledEvent>(new()
                {
                    Staff = staff,
                    User = user,
                });

                return;
            }

            // How many times to progress (needs this many locs)
            int maxProgress = 5;
            // If the oath is done, raise the completion event
            if (progress >= maxProgress)
            {
                RaiseLocalEvent<HippocraticOathCompleteEvent>(new()
                {
                    Staff = staff,
                    User = user,
                });

                return;
            }

            // Continue the oath
            _chatSystem.TrySendInGameICMessage(user, Loc.GetString($"asclepius-binding-hippocratic-oath-progress-{progress}"), InGameICChatType.Speak, false);

            component.CancelToken = new();
            DoAfterEventArgs doafter = new(user, 3f, component.CancelToken.Token)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                BreakOnStun = true,
                MovementThreshold = 0.25f,
                BroadcastFinishedEvent = progress + 1 >= maxProgress ?
                new HippocraticOathCompleteEvent()
                {
                    Staff = staff,
                    User = user,
                } :
                null,
                BroadcastCancelledEvent = new HippocraticOathCancelledEvent()
                {
                    Staff = staff,
                    User = user,
                }
            };
            await _doAfter.WaitDoAfter(doafter);

            if (component.CancelToken != null) Progress(staff, user, progress + 1);
        }
    }
}
