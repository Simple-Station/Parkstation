using Content.Server.Power.Components;
using Content.Server.Storage.Components;
using Content.Shared.PowerCell.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using Content.Server.Popups;
using Robust.Shared.Player;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class SiliconchargerCompSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconChargerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<SiliconChargerComponent, EndCollideEvent>(OnEndCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (chargerComp, entStorage) in EntityManager.EntityQuery<SiliconChargerComponent, EntityStorageComponent>())
        {
            foreach (var entity in entStorage.Contents.ContainedEntities)
            {
                var chargeRate = chargerComp.ChargeMulti * frameTime * 10;

                HandleChargingEntity(entity, chargeRate, chargerComp, frameTime);

                // Heat up the air in the charger.
                if (entStorage.Airtight)
                {
                    var curTemp = entStorage.Air.Temperature;

                    entStorage.Air.Temperature += curTemp < chargerComp.TargetTemp ? frameTime * chargerComp.ChargeMulti / 100 : 0;
                }
            }
        }

        // Check for any chargers with the StepTriggerComponent.
        foreach (var (chargerComp, stepComp) in EntityManager.EntityQuery<SiliconChargerComponent, StepTriggerComponent>())
        {
            if (chargerComp.PresentEntities.Count == 0)
                continue;

            var chargeRate = frameTime * chargerComp.ChargeMulti / chargerComp.PresentEntities.Count ;

            foreach (var entity in chargerComp.PresentEntities)
            {
                HandleChargingEntity(entity, chargeRate, chargerComp, frameTime);
            }
        }
    }

    private void HandleChargingEntity(EntityUid entity, float chargeRate, SiliconChargerComponent chargerComp, float frameTime, bool burn = true)
    {
        // If the given entity has a battery, charge it.
        if (EntityManager.TryGetComponent(entity, out BatteryComponent? batteryComp))
        {
            if (batteryComp.CurrentCharge + chargeRate < batteryComp.MaxCharge)
                batteryComp.CurrentCharge += chargeRate;
            else
                batteryComp.CurrentCharge = batteryComp.MaxCharge;
        }
        // If the given entity contains a battery, charge it.
        else if (EntityManager.TryGetComponent(entity, out PowerCellSlotComponent? cellSlotComp) &&
                _itemSlotsSystem.TryGetSlot(entity, cellSlotComp.CellSlotId, out var slot) &&
                EntityManager.TryGetComponent<BatteryComponent>(slot.Item, out var cellComp))
        {
            if (cellComp.CurrentCharge + chargeRate < cellComp.MaxCharge)
                cellComp.CurrentCharge += chargeRate;
            else
                cellComp.CurrentCharge = cellComp.MaxCharge;
        }
        // If the given entity DOESN'T have a battery, burn the fucker.
        else if (burn &&
                EntityManager.TryGetComponent<DamageableComponent>(entity, out var damageComp) &&
                damageComp.DamageContainerID == "Biological")
        {
            var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Shock"), frameTime * chargerComp.ChargeMulti / 100);
            var damageDealt = _damageableSystem.TryChangeDamage(entity, damage, false, true, damageComp, chargerComp.Owner);
            chargerComp.warningAccumulator -= frameTime;
            if (damageDealt != null && chargerComp.warningAccumulator <= 0 && damageDealt.Total > 0)
            {
                var popupBurn = Loc.GetString("system-silicon-charger-burn", ("charger", chargerComp.Owner), ("entity", entity));
                _popup.PopupEntity(popupBurn, entity, PopupType.MediumCaution);
                chargerComp.warningAccumulator += 5f;
            }
        }
    }

    // When an entity starts colliding with the charger, add it to the list of entities present on the charger if it has the StepTriggerComponent.
    private void OnStartCollide(EntityUid uid, SiliconChargerComponent component, ref StartCollideEvent args)
    {
        if (!EntityManager.HasComponent<StepTriggerComponent>(uid))
            return;

        var target = args.OtherFixture.Body.Owner;

        if (!component.PresentEntities.Contains(target))
        {
            if (component.PresentEntities.Count >= component.MaxEntities)
            {
                _popup.PopupEntity(Loc.GetString("system-silicon-charger-list-too-big"), target, target);
                return;
            }

            _popup.PopupEntity(Loc.GetString("system-silicon-charger-add-to-list"), target, target);

            component.PresentEntities.Add(target);
        }
    }

    // When an entity stops colliding with the charger, remove it from the list of entities present on the charger.
    private void OnEndCollide(EntityUid uid, SiliconChargerComponent component, ref EndCollideEvent args)
    {
        if (!EntityManager.HasComponent<StepTriggerComponent>(uid))
            return;

        var target = args.OtherFixture.Body.Owner;

        if (component.PresentEntities.Contains(target))
        {
            component.PresentEntities.Remove(target);
        }
    }
}
