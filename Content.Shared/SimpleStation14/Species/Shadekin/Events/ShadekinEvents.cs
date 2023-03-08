using Content.Shared.Actions;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
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


        [DataField("powerCost")]
        public float PowerCost = 35f;

        [DataField("staminaCost")]
        public float StaminaCost = 30f;
    }

    /// <summary>
    ///     Raised when the shadekin darkSwap action is used.
    /// </summary>
    public sealed class ShadekinDarkSwapEvent : InstantActionEvent
    {
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
        public float StaminaCost = 25f;
    }

    /// <summary>
    ///     Raised over network to notify the client that they're going in/out of The Dark.
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

    public sealed class ShadekinRestEvent: InstantActionEvent
    {

    }

    [Serializable, NetSerializable]
    public sealed class ShadekinRestEventResponse : EntityEventArgs
    {
        public EntityUid Performer { get; }
        public bool IsResting { get; }

        public ShadekinRestEventResponse(EntityUid performer, bool isResting)
        {
            Performer = performer;
            IsResting = isResting;
        }
    }


    /// <summary>
    ///     Raised when a shadekin becomes a blackeye.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadekinBlackeyeEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;

        public ShadekinBlackeyeEvent(EntityUid euid)
        {
            Euid = euid;
        }
    }
}
