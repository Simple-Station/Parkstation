namespace Content.Server.Slippery
{
    /// <summary>
    ///     Uses provided chance to try and drop the item when slipped, if equipped.
    /// </summary>
    [RegisterComponent]
    public sealed class DropOnSlipComponent : Component
    {
        [DataField("chance")]
        [ViewVariables(VVAccess.ReadWrite)]
        public int Chance = 20;
    }
}
