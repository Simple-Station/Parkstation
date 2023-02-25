namespace Content.Shared.SimpleStation14.Species.Shadekin.Components
{
    [RegisterComponent]
    public sealed class ShadekinLightComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public float OldRadius = 0f;

        [ViewVariables(VVAccess.ReadOnly)]
        public bool OldRadiusEdited = false;


        [ViewVariables(VVAccess.ReadOnly)]
        public float OldEnergy = 0f;

        [ViewVariables(VVAccess.ReadOnly)]
        public bool OldEnergyEdited = false;
    }
}
