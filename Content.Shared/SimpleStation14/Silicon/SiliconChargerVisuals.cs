using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Silicon;

[Serializable, NetSerializable]
public enum SiliconChargerVisuals
{
    Lights,
}

[Serializable, NetSerializable]
public enum SiliconChargerVisualState
{
    Normal,
    NormalOpen,
    Charging
}
