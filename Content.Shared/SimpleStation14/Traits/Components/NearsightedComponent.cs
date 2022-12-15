using Robust.Shared.GameStates;

namespace Content.Shared.Abilities
{
    [RegisterComponent]
    [NetworkedComponent]

    public sealed class NearsightedComponent : Component
    {
        [DataField("radius"), ViewVariables(VVAccess.ReadWrite)]
        public float Radius = 0.9f;

        [DataField("alpha"), ViewVariables(VVAccess.ReadWrite)]
        public float Alpha = 0.995f;

        [DataField("gradius"), ViewVariables(VVAccess.ReadWrite)]
        public float gRadius = 0.75f;

        [DataField("galpha"), ViewVariables(VVAccess.ReadWrite)]
        public float gAlpha = 0.95f;
    }
}
