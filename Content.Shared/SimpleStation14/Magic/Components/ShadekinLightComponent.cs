namespace Content.Shared.SimpleStation14.Magic.Components
{
    [RegisterComponent]
    public sealed class ShadekinLightComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public float OldRadius = 5f;

        [ViewVariables(VVAccess.ReadOnly)]
        public bool OldRadiusEdited = false;
    }
}
