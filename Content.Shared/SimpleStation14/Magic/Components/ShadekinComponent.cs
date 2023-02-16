using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Magic.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed class ShadekinComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Darken = true;

        [ViewVariables(VVAccess.ReadWrite)]
        public float DarkenRange = 5f;

        [ViewVariables(VVAccess.ReadOnly)]
        public List<EntityUid> DarkenedLights = new();


        [ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorTime = 1f;

        [ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;
    }
}
