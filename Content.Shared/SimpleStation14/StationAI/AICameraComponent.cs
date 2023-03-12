using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.StationAI
{
    [RegisterComponent]
    public sealed class AICameraComponent : Component
    {
        [DataField("enabled"), ViewVariables(VVAccess.ReadWrite)]
        public bool Enabled = true;

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
