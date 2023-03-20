namespace Content.Server.Borgs
{
    [RegisterComponent]
    public sealed class InnateItemComponent : Component
    {
        public bool AlreadyInitialized = false;

        [DataField("afterInteract")]
        public bool AfterInteract = true;
    }
}
