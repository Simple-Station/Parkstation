using System.Threading;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

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
                _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-bound"), args.User, PopupType.MediumCaution);
                return;
            }

            // Only humanoids can bind
            if (!_entityManager.TryGetComponent<HumanoidAppearanceComponent>(args.User, out var _))
            {
                _popupSystem.PopupEntity(Loc.GetString("asclepius-binding-inhuman"), args.User, PopupType.MediumCaution);
                return;
            }


            // Begin the oath
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
            int maxProgress = 4;
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
            await Task.Delay(1000);

            component.CancelToken = new();
            DoAfterEventArgs doafter = new(user, 2.25f * (progress + 1), component.CancelToken.Token)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                BreakOnStun = true,
                MovementThreshold = 0.25f,
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
