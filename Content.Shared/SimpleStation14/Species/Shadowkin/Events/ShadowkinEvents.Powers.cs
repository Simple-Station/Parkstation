using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Events;

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
