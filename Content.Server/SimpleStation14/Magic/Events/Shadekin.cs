using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.Magic.Events;

public sealed class ShadekinTeleportEvent : WorldTargetActionEvent
{
    [DataField("blinkSound")]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");


    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("blinkVolume")]
    public float BlinkVolume = 5f;
}

public sealed class ShadekinDarkSwapEvent : InstantActionEvent
{

}
