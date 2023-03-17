using Content.Shared.Actions;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Events
{
    /// <summary>
    ///     Raised when the shadowkin teleport action is used.
    /// </summary>
    public sealed class ShadowkinTeleportEvent : WorldTargetActionEvent
    {
        [DataField("sound")]
        public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/base.ogg");

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
        public SoundSpecifier SoundOn = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/disappearance-gradual.ogg");

        [DataField("volumeOn")]
        public float VolumeOn = 5f;

        [DataField("soundOff")]
        public SoundSpecifier SoundOff = new SoundPathSpecifier("/Audio/SimpleStation14/Effects/Shadowkin/Powers/futuristic-ufo.ogg");

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
        public float StaminaCost = 0f;
    }

    /// <summary>
    ///     Raised over network to notify the client that they're going in/out of The Dark.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadowkinDarkSwappedEvent : EntityEventArgs
    {
        public EntityUid Performer { get; }
        public bool DarkSwapped { get; }

        public ShadowkinDarkSwappedEvent(EntityUid performer, bool darkSwapped)
        {
            Performer = performer;
            DarkSwapped = darkSwapped;
        }
    }

    public sealed class ShadowkinRestEvent: InstantActionEvent
    {

    }

    [Serializable, NetSerializable]
    public sealed class ShadowkinRestEventResponse : EntityEventArgs
    {
        public EntityUid Performer { get; }
        public bool IsResting { get; }

        public ShadowkinRestEventResponse(EntityUid performer, bool isResting)
        {
            Performer = performer;
            IsResting = isResting;
        }
    }


    /// <summary>
    ///     Raised when a shadowkin becomes a blackeye.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadowkinBlackeyeEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;

        public ShadowkinBlackeyeEvent(EntityUid euid)
        {
            Euid = euid;
        }
    }
}
