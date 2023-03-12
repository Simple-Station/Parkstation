namespace Content.Shared.SimpleStation14.Species.Shadekin.Components
{
    [RegisterComponent]
    public sealed class ShadekinRestPowerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsResting = false;
    }
}
