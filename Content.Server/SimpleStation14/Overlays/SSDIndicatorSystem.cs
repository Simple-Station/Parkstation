using Content.Server.Mind.Components;
using Content.Shared.SimpleStation14.Overlays.SSDIndicator;

namespace Content.Server.SimpleStation14.Overlays;

public sealed class SSDIndicatorSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entity = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<SSDIndicatorComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<SSDIndicatorComponent, MindRemovedMessage>(OnMindRemoved);
    }


    private void OnMindAdded(EntityUid uid, SSDIndicatorComponent indicator, MindAddedMessage message)
    {
        indicator.IsSSD = false;
        Dirty(indicator);
    }

    private void OnMindRemoved(EntityUid uid, SSDIndicatorComponent indicator, MindRemovedMessage message)
    {
        indicator.IsSSD = true;
        Dirty(indicator);
    }
}
