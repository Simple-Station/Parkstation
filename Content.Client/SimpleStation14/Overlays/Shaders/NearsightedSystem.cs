using Content.Shared.Abilities;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Network;
using Content.Shared.Tag;
using Content.Client.Inventory;

namespace Content.Client.SimpleStation14.Overlays;
public sealed class NearsightedSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private NearsightedOverlay _overlay = default!;
    private NearsightedComponent nearsight = new();

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new Overlays.NearsightedOverlay();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var nearsight in EntityQuery<NearsightedComponent>())
        {
            var sighted = nearsight.Owner;

            var cinv = EnsureComp<ClientInventoryComponent>(sighted);
            cinv.SlotData.TryGetValue("eyes", out var eyes);
            var eyeslot = eyes?.Container?.ContainedEntity;

            if (eyeslot == null) UpdateShader(nearsight);
            else
            {
                EntityUid eyeslo = new();
                eyeslo = (EntityUid) eyeslot;

                var comp = EnsureComp<TagComponent>(eyeslo);
                if (comp.Tags.Contains("GlassesNearsight")) UpdateShaderGlasses(nearsight);
            }
        }
    }


    private void UpdateShader(NearsightedComponent component)
    {
        while (_overlayMan.HasOverlay<NearsightedOverlay>()) _overlayMan.RemoveOverlay(_overlay);
        component.Glasses = false;
        _overlayMan.AddOverlay(_overlay);
    }

    private void UpdateShaderGlasses(NearsightedComponent component)
    {
        while (_overlayMan.HasOverlay<NearsightedOverlay>()) _overlayMan.RemoveOverlay(_overlay);
        component.Glasses = true;
        _overlayMan.AddOverlay(_overlay);
    }
}
