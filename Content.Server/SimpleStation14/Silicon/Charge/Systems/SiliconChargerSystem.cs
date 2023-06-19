using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Storage.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.PowerCell.Components;
using Content.Shared.SimpleStation14.Silicon;
using Content.Shared.SimpleStation14.Silicon.Charge;
using Content.Shared.StepTrigger.Components;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class SiliconChargerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedSiliconChargerSystem _sharedCharger = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconChargerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<SiliconChargerComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<SiliconChargerComponent, ComponentShutdown>(OnChargerShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        #region Entity Storage Chargers
        // Check for any chargers with the EntityStorageComponent.
        var entstorQuery = EntityQueryEnumerator<SiliconChargerComponent, EntityStorageComponent>();
        while (entstorQuery.MoveNext(out var uid, out var chargerComp, out var entStorage))
        {
            var wasActive = chargerComp.Active;
            chargerComp.Active = false;

            if (EntityManager.TryGetComponent<ApcPowerReceiverComponent>(uid, out var powerComp) && !powerComp.Powered)
            {
                if (chargerComp.Active != wasActive)
                    _sharedCharger.UpdateState(uid, chargerComp);

                continue;
            }

            foreach (var entity in entStorage.Contents.ContainedEntities)
            {
                chargerComp.Active = true;

                var chargeRate = chargerComp.ChargeMulti * frameTime * 10;

                HandleChargingEntity(entity, chargeRate, chargerComp, uid, frameTime);

                // Heat up the air in the charger.
                if (entStorage.Airtight)
                {
                    var curTemp = entStorage.Air.Temperature;

                    entStorage.Air.Temperature += curTemp < chargerComp.TargetTemp ? frameTime * chargerComp.ChargeMulti / 100 : 0;
                }
            }

            if (chargerComp.Active != wasActive)
                _sharedCharger.UpdateState(uid, chargerComp);
        }
        #endregion Entity Storage Chargers

        #region Step Trigger Chargers
        // Check for any chargers with the StepTriggerComponent.
        var stepQuery = EntityQueryEnumerator<SiliconChargerComponent, StepTriggerComponent>();
        while (stepQuery.MoveNext(out var uid, out var chargerComp, out _))
        {
            if (chargerComp.PresentEntities.Count == 0 ||
                TryComp<ApcPowerReceiverComponent>(uid, out var powerComp) && !powerComp.Powered)
            {
                if (chargerComp.Active)
                {
                    chargerComp.Active = false;
                    _sharedCharger.UpdateState(uid, chargerComp);
                }
                continue;
            }

            if (!chargerComp.Active)
            {
                chargerComp.Active = true;
                _sharedCharger.UpdateState(uid, chargerComp);
            }

            var chargeRate = frameTime * chargerComp.ChargeMulti / chargerComp.PresentEntities.Count;

            foreach (var entity in chargerComp.PresentEntities)
            {
                HandleChargingEntity(entity, chargeRate, chargerComp, uid, frameTime);
            }
        }
        #endregion Step Trigger Chargers
    }

    // Cleanup the sound stream when the charger is destroyed.
    private void OnChargerShutdown(EntityUid uid, SiliconChargerComponent component, ComponentShutdown args)
    {
        component.SoundStream?.Stop();
    }

    /// <summary>
    ///     Handles working out what entities need to have their batteries charged, or be burnt.
    /// </summary>
    private void HandleChargingEntity(EntityUid entity, float chargeRate, SiliconChargerComponent chargerComp, EntityUid chargerUid, float frameTime, bool burn = true)
    {
        var entitiesToCharge = SearchThroughEntities(entity, burn);

        if (entitiesToCharge.Count == 0)
            return;

        // Now we charge the entities we found.
        chargeRate /= entitiesToCharge.Count;

        foreach (var entityToCharge in entitiesToCharge)
        {
            if (EntityManager.TryGetComponent<BatteryComponent>(entityToCharge, out _))
                ChargeBattery(entityToCharge, EntityManager.GetComponent<BatteryComponent>(entityToCharge), chargeRate, chargerComp, chargerUid);
            else if (EntityManager.TryGetComponent<DamageableComponent>(entityToCharge, out var damageComp))
                BurnEntity(entityToCharge, damageComp, frameTime, chargerComp, chargerUid);
        }
    }

    private List<EntityUid> SearchThroughEntities(EntityUid entity, bool burn = true)
    {
        var entitiesToCharge = new List<EntityUid>();

        // If the given entity has a battery, charge it.
        if (!EntityManager.TryGetComponent<UnremoveableComponent>(entity, out _) &&
            EntityManager.TryGetComponent(entity, out BatteryComponent? batteryComp) &&
            batteryComp.CurrentCharge < batteryComp.MaxCharge)
        {
            entitiesToCharge.Add(entity);
        }

        // If the given entity contains a battery, charge it.
        else if (!EntityManager.TryGetComponent<UnremoveableComponent>(entity, out _) &&
                EntityManager.TryGetComponent(entity, out PowerCellSlotComponent? cellSlotComp) &&
                _itemSlots.TryGetSlot(entity, cellSlotComp.CellSlotId, out var slot) &&
                EntityManager.TryGetComponent<BatteryComponent>(slot.Item, out var cellComp) &&
                cellComp.CurrentCharge < cellComp.MaxCharge)
        {
            entitiesToCharge.Add(slot.Item.Value);
        }

        // If the given entity is fleshy, burn the fucker.
        else if (burn &&
                EntityManager.TryGetComponent<DamageableComponent>(entity, out var damageComp) &&
                damageComp.DamageContainerID == "Biological")
        {
            entitiesToCharge.Add(entity);
        }

        // Now the weird part, we check for any inventories the entities contained may have, and run this function on any entities contained, for a recursive charging effect.
        if (EntityManager.TryGetComponent<HandsComponent>(entity, out var handsComp))
        {
            foreach (var heldEntity in _hands.EnumerateHeld(entity, handsComp))
            {
                entitiesToCharge.AddRange(SearchThroughEntities(heldEntity));
            }
        }
        if (EntityManager.TryGetComponent<InventoryComponent>(entity, out var inventoryComp))
        {
            foreach (var slot in _inventory.GetSlots(entity, inventoryComp))
            {
                if (_inventory.TryGetSlotEntity(entity, slot.Name, out var slotItem))
                    entitiesToCharge.AddRange(SearchThroughEntities(slotItem.Value));
            }
        }
        if (EntityManager.TryGetComponent<ServerStorageComponent>(entity, out var storageComp))
        {
            foreach (var containedEntity in storageComp.StoredEntities!)
            {
                entitiesToCharge.AddRange(SearchThroughEntities(containedEntity));
            }
        }
        if (EntityManager.TryGetComponent<EntityStorageComponent>(entity, out var entStorage))
        {
            foreach (var containedEntity in entStorage.Contents.ContainedEntities)
            {
                entitiesToCharge.AddRange(SearchThroughEntities(containedEntity));
            }
        }

        return entitiesToCharge;
    }

    private void ChargeBattery(EntityUid entity, BatteryComponent batteryComp, float chargeRate, SiliconChargerComponent chargerComp, EntityUid chargerUid)
    {
        // Do some math so a charger never charges a battery from zero to full in less than 10 seconds, just for the effect of it.
        if (chargerComp.ChargeMulti * 10 > batteryComp.MaxCharge / 10)
        {
            chargeRate /= chargerComp.ChargeMulti * 10 / (batteryComp.MaxCharge / 10);
        }

        if (batteryComp.CurrentCharge + chargeRate < batteryComp.MaxCharge)
            _battery.SetCharge(entity, batteryComp.CurrentCharge + chargeRate, batteryComp);
        else
            _battery.SetCharge(entity, batteryComp.MaxCharge, batteryComp);

        // If the battery is too small, explode it.
        if ((batteryComp.MaxCharge - batteryComp.CurrentCharge) * 1.2 + batteryComp.MaxCharge < chargerComp.MinChargeSize)
        {
            if (EntityManager.TryGetComponent<ExplosiveComponent>(entity, out var explosiveComp))
                _explosion.TriggerExplosive(entity, explosiveComp);
            else
                _explosion.QueueExplosion(entity, "Default", batteryComp.MaxCharge / 50, 1.5f, 200, user: chargerUid);
        }
    }

    private void BurnEntity(EntityUid entity, DamageableComponent damageComp, float frameTime, SiliconChargerComponent chargerComp, EntityUid chargerUid)
    {
        var damage = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>(chargerComp.DamageType), frameTime * chargerComp.ChargeMulti / 100);
        var damageDealt = _damageable.TryChangeDamage(entity, damage, false, true, damageComp, chargerUid);
        chargerComp.WarningAccumulator -= frameTime;

        if (damageDealt != null && chargerComp.WarningAccumulator <= 0 && damageDealt.Total > 0)
        {
            var popupBurn = Loc.GetString("silicon-charger-burn", ("charger", chargerUid), ("entity", entity));
            _popup.PopupEntity(popupBurn, entity, PopupType.MediumCaution);
            chargerComp.WarningAccumulator += 5f;
        }
    }

    #region Charger specific
    #region Step Trigger Chargers
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
                _popup.PopupEntity(Loc.GetString("silicon-charger-list-too-big"), target, target);
                return;
            }

            _popup.PopupEntity(Loc.GetString("silicon-charger-add-to-list"), target, target);

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
    #endregion Step Trigger Chargers
    #endregion Charger specific
}
