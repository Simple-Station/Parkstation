using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Server.Remotes
{
    [RegisterComponent]
    [Access(typeof(DoorRemoteSystem))]
    public sealed class DoorRemoteComponent : Component
    {
        [DataField("defaultMode", customTypeSerializer: typeof(EnumSerializer))]
        public Enum Mode = OperatingMode.OpenClose;
    }

    public enum OperatingMode : byte
    {
        OpenClose,
        ToggleBolts,
        ToggleEmergencyAccess
    }
}
