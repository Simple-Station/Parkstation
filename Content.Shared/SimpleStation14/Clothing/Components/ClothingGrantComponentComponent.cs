namespace Content.Shared.SimpleStation14.Magic
{
    [RegisterComponent]
    public sealed class ClothingGrantComponentComponent : Component
    {
        [DataField("component"), ViewVariables(VVAccess.ReadWrite)]
        public string? Component = null;
        [DataField("tag"), ViewVariables(VVAccess.ReadWrite)]
        public string? Tag = null;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
