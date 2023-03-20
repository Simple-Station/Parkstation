using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.StationAI
{
    [RegisterComponent, NetworkedComponent]
    public sealed class AICameraComponent : Component
    {
        [DataField("enabled"), ViewVariables(VVAccess.ReadWrite)]
        public bool Enabled = false;

        [DataField("cameraName"), ViewVariables(VVAccess.ReadWrite)]
        public string CameraName = "Error";
    }

    [Serializable, NetSerializable]
    public sealed class AICameraComponentState : ComponentState
    {
        public bool Enabled { get; init; }
        public string CameraName { get; init; } = "Error";
    }
}
