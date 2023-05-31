using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Robust.Shared.Random;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Interaction.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using Content.Shared.Throwing;

namespace Content.Server.SimpleStation14.Slippery;

public sealed class DropOnSlipSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _invSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    private static readonly float PocketDropChance = 10f;
    private static readonly float PocketThrowChance = 5f;

    private static readonly float ClumsyDropChance = 5f;
    private static readonly float ClumsyThrowChance = 90f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, ParkSlipEvent>(HandleSlip);
    }


    private void HandleSlip(EntityUid entity, InventoryComponent invComp, ParkSlipEvent args)
    {
        if (!_invSystem.TryGetSlots(entity, out var slotDefinitions, invComp))
            return;

        foreach (var slot in slotDefinitions)
        {
            if (!_invSystem.TryGetSlotEntity(entity, slot.Name, out var item))
                continue;

            // A check for DropOnSlipComponent.
            if (slot.Name != "pocket1" && slot.Name != "pocket2" && EntityManager.TryGetComponent<DropOnSlipComponent>(item, out var dropComp) && _random.NextFloat(0, 100) < dropComp.Chance)
            {
                var popupString = Loc.GetString("system-drop-on-slip-text-component", ("name", entity), ("item", item));

                Drop(entity, item.Value, slot.Name, popupString);
                continue;
            }

            // A check for any items in pockets.
            if (slot.Name == "pocket1" | slot.Name == "pocket2" && _random.NextFloat(0, 100) < PocketDropChance)
            {
                var popupString = Loc.GetString("system-drop-on-slip-text-pocket", ("name", entity), ("item", item));

                Drop(entity, item.Value, slot.Name, popupString);
                continue;
            }

            // A check for ClumsyComponent.
            if (slot.Name != "jumpsuit" && _random.NextFloat(0, 100) < ClumsyDropChance && HasComp<ClumsyComponent>(entity))
            {
                var popupString = Loc.GetString("system-drop-on-slip-text-clumsy", ("name", entity), ("item", item));

                Drop(entity, item.Value, slot.Name, popupString);
                continue;
            }
        }
    }

    private void Drop(EntityUid entity, EntityUid item, string slot, string popupString)
    {
        if (!_invSystem.TryUnequip(entity, slot, false, true))
            return;

        EntityManager.TryGetComponent<PhysicsComponent>(entity, out var entPhysComp);

        if (entPhysComp != null)
        {
            var strength = entPhysComp.LinearVelocity.Length / 1.5f;
            Vector2 direction = (_random.Next(-8, 8), _random.Next(-8, 8));

            _throwing.TryThrow(item, direction, strength, entity);
        }

        _popup.PopupEntity(popupString, entity, PopupType.MediumCaution);

        var logMessage = Loc.GetString("system-drop-on-slip-log", ("entity", ToPrettyString(entity)), ("item", ToPrettyString(item)));
        _adminLogger.Add(LogType.Slip, LogImpact.Low, $"{logMessage}");
    }
}
