using Content.Shared.Chemistry.Components;

namespace Content.Server.SimpleStation14.Traits
{
    [RegisterComponent]
    public sealed class TraitRegenReagentComponent : Component
    {
        [DataField("reagents", required: true)]
        public List<TraitRegenReagentObject> Reagents = default!;
    }

    [DataDefinition]
    public sealed class TraitRegenReagentObject : Object
    {
        [DataField("reagent", required: true), ViewVariables(VVAccess.ReadWrite)]
        public string reagent = "";

        [DataField("solution", required: true), ViewVariables(VVAccess.ReadOnly)]
        public Solution solution = default!;

        [DataField("unitsPerUpdate"), ViewVariables(VVAccess.ReadWrite)]
        public float unitsPerUpdate = 0.2f;

        [DataField("accumulator"), ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;

        [DataField("accumulatorTime"), ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorTime = 0f;
    }
}
