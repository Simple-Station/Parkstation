using Content.Shared.Chemistry.Components;

namespace Content.Server.SimpleStation14.Traits
{
    [RegisterComponent]
    public sealed class TraitRegenReagentComponent : Component
    {
        [DataField("reagents")]
        public List<TraitRegenReagentObject> Reagents = new();
    }

    [DataDefinition]
    public sealed class TraitRegenReagentObject : Object
    {
        [DataField("reagent"), ViewVariables(VVAccess.ReadWrite)]
        public string reagent = "Lexorin";

        [DataField("solutionName"), ViewVariables(VVAccess.ReadOnly)]
        public string solutionName = "chemicals";

        [DataField("solution"), ViewVariables(VVAccess.ReadOnly)]
        public Solution solution = new();

        [DataField("unitsPerUpdate"), ViewVariables(VVAccess.ReadWrite)]
        public float unitsPerUpdate = 1f;

        [DataField("accumulator"), ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;

        [DataField("accumulatorTime"), ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorTime = 1f;
    }
}
