using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Events;

/// <summary>
///     Raised when the shadowkin teleport action is used.
/// </summary>
public sealed class ShadowkinTeleportEvent : WorldTargetActionEvent
{
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/teleport.ogg");

    [DataField("volume")]
    public float Volume = 5f;


    [DataField("powerCost")]
    public float PowerCost = 35f;

    [DataField("staminaCost")]
    public float StaminaCost = 30f;
}

/// <summary>
///     Raised when the shadowkin darkSwap action is used.
/// </summary>
public sealed class ShadowkinDarkSwapEvent : InstantActionEvent
{
    [DataField("soundOn")]
    public SoundSpecifier SoundOn = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/darkswapon.ogg");

    [DataField("volumeOn")]
    public float VolumeOn = 5f;

    [DataField("soundOff")]
    public SoundSpecifier SoundOff = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/darkswapoff.ogg");

    [DataField("volumeOff")]
    public float VolumeOff = 5f;


    /// <summary>
    ///     How much stamina to drain when darkening.
    /// </summary>
    [DataField("powerCostOn")]
    public float PowerCostOn = 45f;

    /// <summary>
    ///     How much stamina to drain when lightening.
    /// </summary>
    [DataField("powerCostOff")]
    public float PowerCostOff = 35f;

    [DataField("staminaCost")]
    public float StaminaCost;
}


public sealed class ShadowkinRestEvent: InstantActionEvent
{

}
