using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.StationAI.Events
{
    public sealed class AIHealthOverlayEvent : InstantActionEvent
    {
        public AIHealthOverlayEvent()
        {

        }
    }

    [Serializable, NetSerializable]
    public sealed class NetworkedAIHealthOverlayEvent : EntityEventArgs
    {
        public EntityUid Performer = EntityUid.Invalid;

        public NetworkedAIHealthOverlayEvent(EntityUid performer)
        {
            Performer = performer;
        }
    }
}
