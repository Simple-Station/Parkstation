namespace Content.Server.SimpleStation14.Species.Shadowkin.Components;

[RegisterComponent]
public sealed class ShadowkinRestPowerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsResting = false;
}
