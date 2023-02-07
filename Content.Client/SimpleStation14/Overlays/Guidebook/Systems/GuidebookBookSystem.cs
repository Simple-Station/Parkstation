using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;

namespace Content.Client.Guidebook;

public sealed class GuidebookBookSystem : EntitySystem
{
    [Dependency] private readonly GuidebookSystem _guideSystem = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<GuidebookBookComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, GuidebookBookComponent component, UseInHandEvent args)
    {
        _guideSystem.OpenGuidebook(component.Guides, includeChildren: component.IncludeChildren, selected: component.Guides[0]);
    }
}
