namespace Content.Shared.SimpleStation14.Species.Shadekin.Components
{
    [RegisterComponent]
    public sealed class ShadekinRestComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsResting = false;
    }
}
