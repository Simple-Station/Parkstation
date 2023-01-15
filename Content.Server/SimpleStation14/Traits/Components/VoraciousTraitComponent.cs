namespace Content.Shared.SimpleStation14.Traits
{
    [RegisterComponent]
    public sealed class VoraciousTraitComponent : Component
    {
        [DataField("negateHunger"), ViewVariables(VVAccess.ReadWrite)]
        public float? NegateHunger;

        [DataField("negateThirst"), ViewVariables(VVAccess.ReadWrite)]
        public float? NegateThirst;
    }
}
