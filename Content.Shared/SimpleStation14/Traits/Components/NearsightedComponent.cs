namespace Content.Shared.SimpleStation14.Traits
{
    /// <summary>
    ///     Owner entity cannot see well, without prescription glasses.
    /// </summary>
    [RegisterComponent]
    public sealed class NearsightedComponent : Component
    {
        [DataField("radius"), ViewVariables(VVAccess.ReadWrite)]
        public float Radius = 0.8f;

        [DataField("alpha"), ViewVariables(VVAccess.ReadWrite)]
        public float Alpha = 0.995f;

        [DataField("gradius"), ViewVariables(VVAccess.ReadWrite)]
        public float gRadius = 0.45f;

        [DataField("galpha"), ViewVariables(VVAccess.ReadWrite)]
        public float gAlpha = 0.93f;

        [DataField("glasses"), ViewVariables(VVAccess.ReadWrite)]
        public bool Glasses = false;
    }
}
