
using Robust.Shared.Serialization;

namespace Content.Shared.Traits
{
    [Serializable, NetSerializable]
    public class TraitAddedEvent : EntityEventArgs
    {
        public readonly EntityUid Uid;
        public readonly List<string> Traits;

        public TraitAddedEvent(EntityUid uid, List<string> traits)
        {
            Uid = uid;
            Traits = traits;
        }
    }
}
