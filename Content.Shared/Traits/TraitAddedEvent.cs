
using Robust.Shared.Serialization;

namespace Content.Shared.Traits
{
    [Serializable, NetSerializable]
    public class TraitAddedEvent : EntityEventArgs
    {
        public readonly EntityUid Uid;
        public readonly string Trait;

        public TraitAddedEvent(EntityUid uid, string trait)
        {
            Uid = uid;
            Trait = trait;
        }
    }
}
