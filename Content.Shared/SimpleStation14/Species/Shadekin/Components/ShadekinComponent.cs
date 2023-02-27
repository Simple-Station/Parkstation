using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed class ShadekinComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Darken = false;

        [ViewVariables(VVAccess.ReadWrite)]
        public float DarkenRange = 5f;

        [ViewVariables(VVAccess.ReadOnly)]
        public List<EntityUid> DarkenedLights = new();


        [ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorTime = 1f;

        [ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;


        [ViewVariables(VVAccess.ReadOnly)]
        public Vector3 TintColor = new(0.5f, 0f, 0.5f);

        [ViewVariables(VVAccess.ReadWrite)]
        public float TintIntensity = 0.25f;
    }
}
