namespace Content.Shared.SimpleStation14.StationAI
{
    [RegisterComponent]
    public sealed class StationAIComponent : Component
    {
        [DataField("action")]
        public string Action = "AIHealthOverlay";
    }
}
