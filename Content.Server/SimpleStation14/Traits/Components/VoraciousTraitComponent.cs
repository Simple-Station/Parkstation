namespace Content.Shared.SimpleStation14.Traits
{
    /// <summary>
    ///     Owner entity can eat/drink much faster.
    /// </summary>
    [RegisterComponent]
    public sealed class VoraciousTraitComponent : Component
    {
        [DataField("negateHunger"), ViewVariables(VVAccess.ReadWrite)]
        public float? NegateHunger;

        [DataField("negateThirst"), ViewVariables(VVAccess.ReadWrite)]
        public float? NegateThirst;
    }
}
