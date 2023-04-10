using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Announcements.Prototypes
{
    /// <summary>
    ///     Defines an announcer and their announcement file paths
    /// </summary>
    [Prototype("announcer")]
    public sealed class AnnouncerPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("name")]
        public string Name { get; } = default!;

        [DataField("basePath")]
        public string BasePath { get; } = default!;

        [DataField("announcementPaths")]
        public AnnouncementData[] AnnouncementPaths { get; } = default!;
    }

    /// <summary>
    ///     Defines a path to an announcement file and that announcement's ID
    /// </summary>
    [DataDefinition]
    public sealed class AnnouncementData
    {
        [DataField("id")]
        public string ID { get; set; } = default!;

        [DataField("path")]
        public string? Path { get; set; } = null;

        [DataField("collection")]
        public string? Collection { get; set; } = null;

        [DataField("message")]
        public string? MessageOverride { get; set; } = null;

        [DataField("audioParams")]
        public AudioParams? AudioParams { get; set; } = null;
    }
}
