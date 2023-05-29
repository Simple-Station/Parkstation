using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.LoudSpeakers;

[RegisterComponent]
public sealed class LoudSpeakerComponent : Component
{
    public IPlayingAudioStream? CurrentPlayingSound;

    public TimeSpan NextPlayTime = TimeSpan.Zero;

    /// <summary>
    ///     The port to look for a signal on.
    /// </summary>
    public string PlaySoundPort = "Trigger";

    /// <summary>
    ///     Whether or not this loudspeaker has ports.
    /// </summary>
    [DataField("ports")]
    public bool Ports = true;

    /// <summary>
    ///     The name of the container the payload is in.
    ///     This is specified in the construction graph.
    /// </summary>
    [DataField("containerSlot")]
    public string ContainerSlot = "payload";

    /// <summary>
    ///     Can this loudspeaker be triggered by interacting with it?
    /// </summary>
    [DataField("triggerOnInteract")]
    public bool TriggerOnInteract = false;

    /// <summary>
    ///     Should this loudspeaker interrupt already playing sounds if triggered?
    ///     If false, the sounds will overlap.
    /// </summary>
    /// <remarks>
    ///     Warning: If this is false, the speaker will not clean up after itself properly.
	///		Since it only saves one sound at a time Use with caution.
	/// </remarks>
    [DataField("interrupt")]
    public bool Interrupt = true;

    /// <summary>
    ///     Cool down time between playing sounds.
    /// </summary>
    [DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    ///     The sound to play if no other sound is specified.
    /// </summary>
    [DataField("defaultSound")]
    public SoundSpecifier DefaultSound = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/metaldink.ogg");

    /// <summary>
    ///     Default variance to be used, if no other variance is specified.
    ///     Is still subject to <see cref="VarianceMod"/>.
    /// </summary>
    [DataField("defaultVariance")]
    public float DefaultVariance = 0.125f;

    /// <summary>
    ///     The amount to multiply the volume by.
    /// </summary>
    [DataField("volumeMod")]
    public float VolumeMod = 3.5f;

    /// <summary>
    ///     The amount to multiply the range by.
    /// </summary>
    [DataField("rangeMod")]
    public float RangeMod = 3.5f;

    /// <summary>
    ///     The amount to multiply the rolloff by.
    /// </summary>
    [DataField("rolloffMod")]
    public float RolloffMod = 0.3f;

    /// <summary>
    ///     Amount to multiply the variance by, if the sound has one.
    ///     If the sound has a variance of 0, default variance is used.
    /// </summary>
    [DataField("varianceMod")]
    public float VarianceMod = 1.5f;
}
