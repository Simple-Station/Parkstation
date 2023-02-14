using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Clothing
{
    /// <summary>
    ///     Grants the owner entity the specified component while equipped.
    [RegisterComponent]
    public sealed class ClothingGrantComponentComponent : Component
    {
        [DataField("component", required: true)]
        [AlwaysPushInheritance]
        public EntityPrototype.ComponentRegistry Components { get; } = new();

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
