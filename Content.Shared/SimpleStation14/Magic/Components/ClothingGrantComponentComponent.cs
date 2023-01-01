namespace Content.Shared.SimpleStation14.Magic
{
    [RegisterComponent]
    public sealed class ClothingGrantComponentComponent : Component
    {
        [DataField("component", required: true), ViewVariables(VVAccess.ReadWrite)]
        public string Component = "";

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
