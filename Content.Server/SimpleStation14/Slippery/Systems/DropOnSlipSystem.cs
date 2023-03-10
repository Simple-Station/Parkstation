using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Item;
using Content.Shared.Inventory;
using Robust.Shared.Random;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Interaction.Components;

namespace Content.Server.Slippery;
public sealed class DropOnSlipSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InventoryComponent, SlipEvent>(HandleSlip);
    }

    private void HandleSlip(EntityUid uid, InventoryComponent inventoryComponent, SlipEvent args)
    {
        var invSystem = _entities.System<InventorySystem>();
        if (invSystem.TryGetSlots(uid, out var slotDefinitions, inventoryComponent))
        {
            foreach (var slot in slotDefinitions)
            {
                if (invSystem.TryGetSlotEntity(uid, slot.Name, out var item))
                {
                    _entities.TryGetComponent<ItemComponent>(item, out var itemComp);
                    if (_entities.TryGetComponent<DropOnSlipComponent>(item, out var dropComp) && slot.Name != "pocket1" && slot.Name != "pocket2" && (_random.NextFloat(0, 100) < dropComp.Chance))
                    {
                        invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                        _popupSystem.PopupEntity(Loc.GetString("system-drop-on-slip-text-component", ("name", inventoryComponent.Owner), ("item", item)), uid, PopupType.MediumCaution);
                        _adminLogger.Add(LogType.Slip, LogImpact.Low,
                            $"{ToPrettyString(uid):mob} dropped {ToPrettyString((EntityUid) item):entity} when slipping");
                    }
                    else if (_random.NextFloat(0, 100) < 5 && _entities.TryGetComponent<ClumsyComponent>(uid, out var ____) && slot.Name != "jumpsuit")
                    {
                        invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                        _popupSystem.PopupEntity(Loc.GetString("system-drop-on-slip-text-clumsy", ("name", inventoryComponent.Owner), ("item", item)), uid, PopupType.Medium);
                        _adminLogger.Add(LogType.Slip, LogImpact.Low,
                            $"{ToPrettyString(uid):mob} dropped {ToPrettyString((EntityUid) item):entity} when slipping");
                    }
                    else if (_random.NextFloat(0, 100) < 10 && slot.Name == "pocket1" | slot.Name == "pocket2")
                    {
                        invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                        _popupSystem.PopupEntity(Loc.GetString("system-drop-on-slip-text-pocket", ("name", inventoryComponent.Owner), ("item", item)), uid, PopupType.MediumCaution);
                        _adminLogger.Add(LogType.Slip, LogImpact.Low,
                            $"{ToPrettyString(uid):mob} dropped {ToPrettyString((EntityUid) item):entity} when slipping");
                    }
                }
            }
        }
    }
}
