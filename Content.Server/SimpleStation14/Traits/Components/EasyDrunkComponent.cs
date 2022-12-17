namespace Content.Server.SimpleStation14.Traits
{
    [RegisterComponent]
    public sealed class EasyDrunkComponent : Component
    {
        [DataField("reagent"), ViewVariables(VVAccess.ReadOnly)]
        public TraitRegenReagentObject Reagent = new();
    }
}
