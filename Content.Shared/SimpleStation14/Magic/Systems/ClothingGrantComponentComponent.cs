using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;

namespace Content.Shared.SimpleStation14.Magic;

public sealed class ClothingGrantComponentSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingGrantComponentComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<ClothingGrantComponentComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, ClothingGrantComponentComponent component, GotEquippedEvent args)
    {
        // This only works on clothing
        if (!TryComp<ClothingComponent>(uid, out var clothing)) return;

        // Is the clothing in its actual slot?
        if (!clothing.Slots.HasFlag(args.SlotFlags)) return;

        // does the user already has this component?
        var componentType = _componentFactory.GetRegistration(component.Component).Type;
        if (EntityManager.HasComponent(args.Equipee, componentType)) return;

        var newComponent = (Component) _componentFactory.GetComponent(componentType);
        newComponent.Owner = args.Equipee;

        EntityManager.AddComponent(args.Equipee, newComponent);

        component.IsActive = true;
    }

    private void OnUnequip(EntityUid uid, ClothingGrantComponentComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive) return;

        component.IsActive = false;
        var componentType = _componentFactory.GetRegistration(component.Component).Type;
        if (EntityManager.HasComponent(args.Equipee, componentType))
        {
            EntityManager.RemoveComponent(args.Equipee, componentType);
        }
    }
}
