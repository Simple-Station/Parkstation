using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Documentation
{
        /// <summary>
    /// Key representing which <see cref="BoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum DocsWindowUIKey : byte
    {
        Key,
    }
}
