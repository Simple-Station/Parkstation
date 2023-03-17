namespace Content.Shared.SimpleStation14.Species.Shadowkin.Components
{
    [RegisterComponent]
    public sealed class ShadowkinRestPowerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsResting = false;
    }
}
