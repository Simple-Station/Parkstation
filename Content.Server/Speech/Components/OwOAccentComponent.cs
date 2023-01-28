namespace Content.Server.Speech.Components
{
    [RegisterComponent]
    public sealed class OwOAccentComponent : Component
    {
        [DataField("kaomoji"), ViewVariables(VVAccess.ReadWrite)]
        public bool Kaomoji = true;
    }
}
