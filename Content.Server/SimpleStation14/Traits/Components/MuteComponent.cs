namespace Content.Server.SimpleStation14.Traits
{
    /// <summary>
    /// Owner entity cannot speak.
    /// </summary>
    [RegisterComponent]
    public sealed class MuteTraitComponent : Component
    {
        /// <summary>
        /// Whether this component is active or not.
        /// </summary>
        [ViewVariables]
        [DataField("enabled")]
        public bool Enabled = true;
    }
}
