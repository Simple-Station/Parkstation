namespace Content.Server.Slippery
{
    [RegisterComponent]
    public sealed class DropOnSlipComponent : Component
    {
        [DataField("chance")]
        [ViewVariables(VVAccess.ReadWrite)]
        public int Chance = 20;
    }
}
