using Robust.Shared.GameStates;

namespace Content.Shared.Abilities
{
    [RegisterComponent]
    [NetworkedComponent]

    public sealed class NearsightedComponent : Component
    {
        [DataField("radius"), ViewVariables(VVAccess.ReadWrite)]
        public float Radius = 0.85f;

        [DataField("alpha"), ViewVariables(VVAccess.ReadWrite)]
        public float Alpha = 0.995f;

        [DataField("gradius"), ViewVariables(VVAccess.ReadWrite)]
        public float gRadius = 0.6f;

        [DataField("galpha"), ViewVariables(VVAccess.ReadWrite)]
        public float gAlpha = 0.93f;

        [DataField("glasses"), ViewVariables(VVAccess.ReadWrite)]
        public bool Glasses = false;
    }
}
