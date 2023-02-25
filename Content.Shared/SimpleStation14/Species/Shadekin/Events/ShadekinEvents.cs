using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Events
{
    /// <summary>
    ///     Raised when the shadekin teleport action is used.
    /// </summary>
    public sealed class ShadekinTeleportEvent : WorldTargetActionEvent
    {
        [DataField("blinkSound")]
        public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

        [DataField("blinkVolume")]
        public float BlinkVolume = 5f;


        [DataField("staminaCost")]
        public float StaminaCost = 35f;
    }

    /// <summary>
    ///     Raised when the shadekin darkSwap action is used.
    /// </summary>
    public sealed class ShadekinDarkSwapEvent : InstantActionEvent
    {
        /// <summary>
        ///     How much stamina to drain when darkening.
        /// </summary>
        [DataField("staminaCostOn")]
        public float StaminaCostOn = 45f;

        /// <summary>
        ///     How much stamina to drain when lightening.
        /// </summary>
        [DataField("staminaCostOff")]
        public float StaminaCostOff = 35f;
    }

    /// <summary>
    ///     Raised over network to notify the client that they're going in/out of the dark.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadekinDarkSwappedEvent : EntityEventArgs
    {
        public EntityUid Performer { get; }
        public bool IsDark { get; }

        public ShadekinDarkSwappedEvent(EntityUid performer, bool isDark)
        {
            Performer = performer;
            IsDark = isDark;
        }
    }

    /// <summary>
    ///     Raised when someone gains or loses access to empathy chat.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadekinChangedEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;

        public ShadekinChangedEvent(EntityUid euid)
        {
            Euid = euid;
        }
    }
}
