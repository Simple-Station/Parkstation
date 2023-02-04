using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Client.Guidebook;

/// <summary>
/// This component allows a book to open the guidebook entries specific to it.
/// </summary>
[RegisterComponent]
public sealed class GuidebookBookComponent : Component
{
    /// <summary>
    ///     What guides to include show when opening the guidebook. The first entry will be used to select the currently
    ///     selected guidebook.
    /// </summary>
    [DataField("guides", customTypeSerializer: typeof(PrototypeIdListSerializer<GuideEntryPrototype>), required: true)]
    public List<string> Guides = new();

    /// <summary>
    ///     Whether or not to automatically include the children of the given guides.
    /// </summary>
    [DataField("includeChildren")]
    public bool IncludeChildren = true;
}
