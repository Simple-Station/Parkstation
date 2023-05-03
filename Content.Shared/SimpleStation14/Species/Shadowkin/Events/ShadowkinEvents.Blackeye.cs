using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Events
{
    /// <summary>
    ///     Raised when a shadowkin becomes a blackeye.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShadowkinBlackeyeEvent : EntityEventArgs
    {
        public readonly EntityUid Uid;
        public readonly bool Damage;

        public ShadowkinBlackeyeEvent(EntityUid uid, bool damage = true)
        {
            Uid = uid;
            Damage = damage;
        }
    }
}
