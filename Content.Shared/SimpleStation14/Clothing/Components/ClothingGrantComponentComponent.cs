using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Clothing
{
    /// <summary>
    ///     Grants the owner entity the specified component while equipped.
    /// </summary>
    [RegisterComponent]
    public sealed class ClothingGrantComponentComponent : Component
    {
        [DataField("component", required: true)]
        [AlwaysPushInheritance]
        public ComponentRegistry Components { get; } = new();

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
