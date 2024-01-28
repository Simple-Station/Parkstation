namespace Content.Server.Borgs
{
    [RegisterComponent]
    public sealed class InnateItemComponent : Component
    {
        public bool AlreadyInitialized = false;

        [DataField("startingPriority")]
        public int? StartingPriority = null;
    }
}
