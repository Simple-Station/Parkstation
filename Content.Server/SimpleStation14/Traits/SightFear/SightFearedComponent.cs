namespace Content.Server.SimpleStation14.Traits.SightFear
{
    [RegisterComponent]
    public sealed class SightFearedComponent : Component
    {
        /// <summary>
        ///     The fears this entity inflicts, and their power
        /// </summary>
        [DataField("fears")]
        [ViewVariables(VVAccess.ReadWrite)]
        public Dictionary<string, float> Fears = new();
    }
}
