using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Magic.Events
{
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

    [Serializable, NetSerializable]
    public sealed class ShadekinChangedEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;

        public ShadekinChangedEvent(EntityUid euid)
        {
            Euid = euid;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ShadekinDarkenEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;
        public readonly List<EntityUid> Lights;

        public ShadekinDarkenEvent(EntityUid euid, List<EntityUid> lights)
        {
            Euid = euid;
            Lights = lights;
        }
    }
}
