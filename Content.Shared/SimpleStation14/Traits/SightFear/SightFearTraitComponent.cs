namespace Content.Shared.SimpleStation14.Traits.SightFear
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class SightFearTraitComponent : Component
    {
        // TODO: Check if the ID is valid
        [DataField("afraidOf"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public string AfraidOf = string.Empty;
    }
}
