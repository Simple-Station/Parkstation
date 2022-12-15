using Content.Shared.Abilities;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Network;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Inventory;
using Content.Client.Inventory;
using Content.Client.SimpleStation14.Overlays;

namespace Content.Client.SimpleStation14.Overlays;
public sealed class NearsightedSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly INetManager _net = default!;

    private NearsightedOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new Overlays.NearsightedOverlay();
        _overlayMan.AddOverlay(_overlay);

        SubscribeLocalEvent<NearsightedComponent, ComponentStartup>(OnNearsightedStartup);
        SubscribeLocalEvent<NearsightedComponent, ComponentShutdown>(OnNearsightedShutdown);
        SubscribeLocalEvent<NearsightedComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<NearsightedComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NearsightedComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<NearsightedComponent, DidEquipEvent>(DidEquip);
        SubscribeLocalEvent<NearsightedComponent, DidUnequipEvent>(DidUnequip);
    }

    private void OnPlayerAttached(EntityUid uid, NearsightedComponent component, PlayerAttachedEvent args)
    {
        UpdateShader(component);
    }

    private void OnPlayerDetached(EntityUid uid, NearsightedComponent component, PlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNearsightedStartup(EntityUid uid, NearsightedComponent component, ComponentStartup args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            UpdateShader(component);
        }
    }

    private void OnNearsightedShutdown(EntityUid uid, NearsightedComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnExamined(EntityUid uid, NearsightedComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            args.PushMarkup(Loc.GetString("monochromatic-blindness-trait-examined", ("target", Identity.Entity(uid, EntityManager))));
        }
    }

    private void DidEquip(EntityUid uid, NearsightedComponent component, DidEquipEvent args)
    {
        var comp = EnsureComp<TagComponent>(args.Equipment);

        if (comp.Tags.Contains("GlassesNearsight") && args.SlotFlags == SlotFlags.EYES) UpdateShaderGlasses(component);
        else if (args.SlotFlags is not SlotFlags.EYES) UpdateShader(component);
    }

    private void DidUnequip(EntityUid uid, NearsightedComponent component, DidUnequipEvent args)
    {
        var cinv = EnsureComp<ClientInventoryComponent>(args.Equipee);
        cinv.SlotData.TryGetValue("eyes", out var eyes);

        if (eyes?.Container?.ContainedEntity == null) UpdateShader(component);
    }


    private void UpdateShader(NearsightedComponent component)
    {
        _overlayMan.RemoveOverlay(_overlay);
        _overlay.OxygenLevel = component.Radius;
        _overlay.outerDarkness = component.Alpha;
        _overlayMan.AddOverlay(_overlay);
    }

    private void UpdateShaderGlasses(NearsightedComponent component)
    {
        _overlayMan.RemoveOverlay(_overlay);
        _overlay.OxygenLevel = component.gRadius;
        _overlay.outerDarkness = component.gAlpha;
        _overlayMan.AddOverlay(_overlay);
    }
}
