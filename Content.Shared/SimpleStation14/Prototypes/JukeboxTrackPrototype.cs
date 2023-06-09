using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Prototypes;

[Prototype("jukeboxTrack")] [Serializable] [NetSerializable]
public sealed class JukeboxTrackPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     Input string for the name, pre-localization.
    /// </summary>
    [DataField("name")]
    private string NameString { get; } = default!;

    /// <summary>
    ///     Actual name, localized.
    /// </summary>
    public string Name => Loc.GetString(NameString);

    /// <summary>
    ///     Path of this track's audio file.
    /// </summary>
    [DataField("path")]
    public SoundSpecifier Path { get; } = default!;

    /// <summary>
    ///     Serialized as a string because TimeSpan is not serializable, and converted to a TimeSpan with <see cref="ToTimeSpan"/>.
    /// </summary>
    [DataField("duration")]
    private string DurationString { get; } = default!;

    /// <summary>
    ///     Actual duration as a TimeSpan.
    /// </summary>
    public TimeSpan Duration => ToTimeSpan(DurationString);

    /// <summary>
    ///     Path of this track's art file.
    /// </summary>
    [DataField("artPath")]
    public string ArtPath { get; } = "/Textures/SimpleStation14/JukeboxTracks/default.png";

    private static TimeSpan ToTimeSpan(string time)
    {
        var split = time.Split(':');
        if (split.Length != 2)
        {
            Logger.Error($"Invalid time format: {time}");
            Logger.Debug($"Duration string should be in the format 'mm:ss'");
            throw new ArgumentException($"Invalid time format: {time}");
        }

        if (!int.TryParse(split[0], out var minutes))
        {
            Logger.Error($"Invalid time format: {time}");
            Logger.Debug($"Invalid minutes: {split[0]}");
            throw new ArgumentException($"Invalid time format: {time}");
        }

        if (!int.TryParse(split[1], out var seconds))
        {
            Logger.Error($"Invalid time format: {time}");
            Logger.Debug($"Invalid seconds: {split[1]}");
            throw new ArgumentException($"Invalid time format: {time}");
        }

        return new TimeSpan(0, minutes, seconds);
    }
}
