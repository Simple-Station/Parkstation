using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.SimpleStation14.SSDIndicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SSDIndicatorComponent : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool IsSSD = true;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public ResPath RsiPath = new ("/Textures/SimpleStation14/Overlays/ssd_indicator.rsi");

    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public string RsiState = "sleepy";
}
