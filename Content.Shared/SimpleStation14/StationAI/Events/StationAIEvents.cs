using System.Numerics;
using Content.Shared.Actions;
using Robust.Shared.Map;
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


    [Serializable, NetSerializable]
    public sealed class AICameraListMessage : BoundUserInterfaceMessage
    {
        public EntityUid Owner;

        public AICameraListMessage(EntityUid owner)
        {
            Owner = owner;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AICameraWarpMessage : BoundUserInterfaceMessage
    {
        public EntityUid Owner;
        public EntityCoordinates Coords;

        public AICameraWarpMessage(EntityUid owner, EntityCoordinates coords)
        {
            Owner = owner;
            Coords = coords;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AIBoundUserInterfaceState : BoundUserInterfaceState
    {
        public List<CameraData> Cameras = new();

        public AIBoundUserInterfaceState(List<CameraData> cameras)
        {
            Cameras = cameras;
        }

        [Serializable, NetSerializable]
        public struct CameraData
        {
            public string Name;
            public EntityCoordinates Coords;
            public bool Active;

            public CameraData(string name, EntityCoordinates coords, bool active)
            {
                Name = name;
                Coords = coords;
                Active = active;
            }
        }
    }
}
