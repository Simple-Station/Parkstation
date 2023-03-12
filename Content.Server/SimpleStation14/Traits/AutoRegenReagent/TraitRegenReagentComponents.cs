namespace Content.Server.SimpleStation14.Traits
{
    public abstract class AbstractTraitRegenReagentComponent : Component
    {
        [DataField("reagent"), ViewVariables(VVAccess.ReadOnly)]
        public TraitRegenReagentObject Reagent = new();
    }


    [RegisterComponent]
    public sealed class DraconicBloodstreamComponent : AbstractTraitRegenReagentComponent
    {

    }

    [RegisterComponent]
    public sealed class EasyDrunkComponent : AbstractTraitRegenReagentComponent
    {

    }
}
