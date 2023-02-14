namespace Content.Shared.Traits.Assorted
{
    /// <summary>
    ///     Adjusts an entities height and zoom.
    /// </summary>
    [RegisterComponent]
    public sealed class HeightAdjustedComponent : Component
    {
        [DataField("height", required: true)]
        public float Height { get; }
    }
}
