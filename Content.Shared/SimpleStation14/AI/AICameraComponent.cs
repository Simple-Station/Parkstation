namespace Content.Shared.SimpleStation14.AI
{
    [RegisterComponent]
    public sealed class AICameraComponent : Component
    {
        [DataField("enabled")]
        public bool Enabled = true;

        [DataField("cameraName")]
        public string CameraName = "Error";
    }
}
