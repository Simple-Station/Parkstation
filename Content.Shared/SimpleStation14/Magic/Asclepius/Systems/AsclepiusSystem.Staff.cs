using Content.Shared.Interaction.Events;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Robust.Shared.Containers;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusStaffSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AsclepiusStaffComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
            SubscribeLocalEvent<AsclepiusStaffComponent, DroppedEvent>(OnDropped);
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
    }
}
