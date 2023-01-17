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

            // Convert this to ClothingGrantComponent Tag, this is pointlessly excessive
            var cinv = EnsureComp<ClientInventoryComponent>(sighted);
            cinv.SlotData.TryGetValue("eyes", out var eyes);
            var eyeslot = eyes?.Container?.ContainedEntity;

            if (eyeslot == null) UpdateShader(nearsight, false);
            else
            {
                EntityUid eyeslo = new();
                eyeslo = (EntityUid) eyeslot;

                var comp = EnsureComp<TagComponent>(eyeslo);
                if (comp.Tags.Contains("GlassesNearsight")) UpdateShader(nearsight, true);
            }
        }
    }


    private void UpdateShader(NearsightedComponent component, bool booLean)
    {
        while (_overlayMan.HasOverlay<NearsightedOverlay>()) _overlayMan.RemoveOverlay(_overlay);
        component.Glasses = booLean;
        _overlayMan.AddOverlay(_overlay);
    }
}
