namespace Content.Shared.SimpleStation14.Traits.SightFear
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class SightFearedComponent : Component
    {
        /// <summary>
        ///     The fears this entity inflicts, and their power
        /// </summary>
        [DataField("fears")]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public Dictionary<string, float> Fears = new();
    }
}
