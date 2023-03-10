namespace Content.Shared.SimpleStation14.Clothing
{
    /// <summary>
    ///     Grants the owner entity the specified tag while equipped.
    /// </summary>
    [RegisterComponent]
    public sealed class ClothingGrantTagComponent : Component
    {
        [DataField("tag", required: true), ViewVariables(VVAccess.ReadWrite)]
        public string Tag = "";

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
