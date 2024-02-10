using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Holograms;

[Serializable, NetSerializable]
public enum AcceptHologramUiButton
{
    Deny,
    Accept,
}

[Serializable, NetSerializable]
public sealed class AcceptHologramChoiceMessage : EuiMessageBase
{
    public readonly AcceptHologramUiButton Button;

    public AcceptHologramChoiceMessage(AcceptHologramUiButton button)
    {
        Button = button;
    }
}
