namespace Content.Server.SimpleStation14.Traits
{
    [RegisterComponent]
    public sealed class DraconicBloodstreamComponent : Component
    {
        [DataField("reagent"), ViewVariables(VVAccess.ReadOnly)]
        public TraitRegenReagentObject Reagent = new();
    }
}
