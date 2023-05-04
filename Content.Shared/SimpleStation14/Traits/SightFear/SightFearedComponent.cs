namespace Content.Shared.SimpleStation14.Traits.SightFear
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class SightFearedComponent : Component
    {
        // TODO: Check if the IDs are valid
        [DataField("fears"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public Dictionary<string, float> Fears = new();
    }
}
