using Content.Shared.MachineLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.SimpleStation14.LoudSpeakers;

[RegisterComponent]
public sealed class LoudSpeakerComponent : Component
{
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
    ///     Can this loudspeaker be triggered by interacting with it?
    /// </summary>
    [DataField("triggerOnInteract")]
    public bool TriggerOnInteract = false;

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
    public float VolumeMod = 2f;

    /// <summary>
    ///     The amount to multiply the range by.
    /// </summary>
    [DataField("rangeMod")]
    public float RangeMod = 3f;

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
    public float VarianceMod = 1.35f;

    /// <summary>
    ///     The modifier to apply to the default variance.

    /// <summary>
    ///     The name of the container the payload is in.
    ///     This is specified in the construction graph.
    /// </summary>
    [DataField("containerSlot")]
    public string ContainerSlot = "payload";
}
