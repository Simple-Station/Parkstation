using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Clothing
{
    [RegisterComponent]
    public sealed class PolychromicComponent : Component
    {

    }
}

[Serializable, NetSerializable]
public enum PolychromicUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PolychromicBoundUserInterfaceState : BoundUserInterfaceState
{
    public PolychromicBoundUserInterfaceState()
    {
    }
}
