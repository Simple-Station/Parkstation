using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Events
{
    [Serializable, NetSerializable]
    public sealed class HippocraticOathCompleteEvent : CancellableEntityEventArgs
    {
        public EntityUid Staff { get; init; } = default!;
        public EntityUid User { get; init; }
    }

    [Serializable, NetSerializable]
    public sealed class HippocraticOathProgressingEvent : CancellableEntityEventArgs
    {
        public EntityUid Staff { get; init; } = default!;
        public EntityUid User { get; init; }
        public int Progress { get; init; }
    }

    [Serializable, NetSerializable]
    public sealed class HippocraticOathCancelledEvent : CancellableEntityEventArgs
    {
        public EntityUid Staff { get; init; } = default!;
        public EntityUid User { get; init; }
    }
}
