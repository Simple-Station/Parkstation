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
using Content.Shared.SimpleStation14.Silicon;
using Robust.Shared.Audio;
using Robust.Server.GameObjects;
using Content.Server.Hands.Components;
using Content.Shared.Inventory;
using Content.Server.Hands.Systems;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Interaction.Components;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class SiliconchargerCompSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;

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

        // Check for any chargers with the EntityStorageComponent.
        foreach (var (chargerComp, entStorage) in EntityManager.EntityQuery<SiliconChargerComponent, EntityStorageComponent>())
        {
            var wasActive = chargerComp.Active;
            chargerComp.Active = false;

            if (EntityManager.TryGetComponent<ApcPowerReceiverComponent>(chargerComp.Owner, out var powerComp) && !powerComp.Powered)
            {
                if (chargerComp.Active != wasActive)
                    UpdateState(chargerComp.Owner, chargerComp);

                continue;
            }

            foreach (var entity in entStorage.Contents.ContainedEntities)
            {
                chargerComp.Active = true;

                var chargeRate = chargerComp.ChargeMulti * frameTime * 10;

                HandleChargingEntity(entity, chargeRate, chargerComp, frameTime);

                // Heat up the air in the charger.
                if (entStorage.Airtight)
                {
                    var curTemp = entStorage.Air.Temperature;

                    entStorage.Air.Temperature += curTemp < chargerComp.TargetTemp ? frameTime * chargerComp.ChargeMulti / 100 : 0;
                }
            }

            if (chargerComp.Active != wasActive)
                UpdateState(chargerComp.Owner, chargerComp);
        }

        // Check for any chargers with the StepTriggerComponent.
        foreach (var (chargerComp, stepComp) in EntityManager.EntityQuery<SiliconChargerComponent, StepTriggerComponent>())
        {
            if (chargerComp.PresentEntities.Count == 0 ||
                (EntityManager.TryGetComponent<ApcPowerReceiverComponent>(chargerComp.Owner, out var powerComp) && !powerComp.Powered))
            {
                if (chargerComp.Active)
                {
                    chargerComp.Active = false;
                    UpdateState(chargerComp.Owner, chargerComp);
                }
                continue;
            }

            if (!chargerComp.Active)
            {
                chargerComp.Active = true;
                UpdateState(chargerComp.Owner, chargerComp);
            }

            var chargeRate = frameTime * chargerComp.ChargeMulti / chargerComp.PresentEntities.Count;

            foreach (var entity in chargerComp.PresentEntities)
            {
                HandleChargingEntity(entity, chargeRate, chargerComp, frameTime);
            }
        }
    }

    // Cleanup the sound stream when the charger is destroyed.
    private void OnChargerShutdown(EntityUid uid, SiliconChargerComponent component, ComponentShutdown args)
    {
        component.SoundStream?.Stop();
    }

    /// <summary>
    ///     Handles working out what entities need to have their batteries charged, or be burnt.
    /// </summary>
    private void HandleChargingEntity(EntityUid entity, float chargeRate, SiliconChargerComponent chargerComp, float frameTime, bool burn = true)
    {
        var entitiesToCharge = SearchThroughEntities(entity, burn);

        if (entitiesToCharge.Count == 0)
            return;

        // Now we charge the entities we found.
        chargeRate /= entitiesToCharge.Count;

        foreach (var entityToCharge in entitiesToCharge)
        {
            if (EntityManager.TryGetComponent<BatteryComponent>(entityToCharge, out var batteryComp))
                ChargeBattery(entityToCharge, EntityManager.GetComponent<BatteryComponent>(entityToCharge), chargeRate, chargerComp);
            else if (EntityManager.TryGetComponent<DamageableComponent>(entityToCharge, out var damageComp))
                BurnEntity(entityToCharge, damageComp, frameTime, chargerComp);
        }
    }

    private List<EntityUid> SearchThroughEntities(EntityUid entity, bool burn = true)
    {
        var entitiesToCharge = new List<EntityUid>();

        // If the given entity has a battery, charge it.
        if (EntityManager.TryGetComponent(entity, out BatteryComponent? batteryComp) &&
            !EntityManager.TryGetComponent<UnremoveableComponent>(entity, out var _) &&
            batteryComp.CurrentCharge < batteryComp.MaxCharge)
        {
            entitiesToCharge.Add(entity);
        }

        // If the given entity contains a battery, charge it.
        else if (EntityManager.TryGetComponent(entity, out PowerCellSlotComponent? cellSlotComp) &&
                !EntityManager.TryGetComponent<UnremoveableComponent>(entity, out var _) &&
                _itemSlotsSystem.TryGetSlot(entity, cellSlotComp.CellSlotId, out var slot) &&
                EntityManager.TryGetComponent<BatteryComponent>(slot.Item, out var cellComp) &&
                cellComp.CurrentCharge < cellComp.MaxCharge)
        {
            entitiesToCharge.Add(slot.Item.Value);
        }

        // If the given entity DOESN'T have a battery, burn the fucker.
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
            foreach ( var containedEntity in storageComp.StoredEntities!)
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

    private void ChargeBattery(EntityUid entity, BatteryComponent batteryComp, float chargeRate, SiliconChargerComponent chargerComp)
    {
        // Do some math so a charger never charges a battery in less than 10 seconds, just for the effect of it.
        if (chargerComp.ChargeMulti * 10 > batteryComp.MaxCharge / 10)
        {
            chargeRate /= (chargerComp.ChargeMulti * 10) / (batteryComp.MaxCharge / 10);
        }

        if (batteryComp.CurrentCharge + chargeRate < batteryComp.MaxCharge)
            batteryComp.CurrentCharge += chargeRate;
        else
            batteryComp.CurrentCharge = batteryComp.MaxCharge;

        // If the battery is too small, explode it.
        if ((batteryComp.MaxCharge - batteryComp.CurrentCharge) * 1.2 + batteryComp.MaxCharge < chargerComp.MinChargeSize)
        {
            if (EntityManager.TryGetComponent<ExplosiveComponent>(entity, out var explosiveComp))
            {
                _explosion.TriggerExplosive(entity, explosiveComp);
            }
            else
            {
                _explosion.QueueExplosion(entity, "Default", batteryComp.MaxCharge / 50, 1.5f, 200, user: chargerComp.Owner);
            }
        }
    }

    private void BurnEntity(EntityUid entity, DamageableComponent damageComp, float frameTime, SiliconChargerComponent chargerComp)
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

    private void UpdateState(EntityUid uid, SiliconChargerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Active)
        {
            _appearance.SetData(uid, SiliconChargerVisuals.Lights, SiliconChargerVisualState.Charging);

            if (component.SoundLoop != null)
                component.SoundStream =
                    _audio.PlayPvs(component.SoundLoop, uid, AudioParams.Default.WithLoop(true).WithMaxDistance(5));
        }
        else
        {
            _appearance.SetData(uid, SiliconChargerVisuals.Lights, SiliconChargerVisualState.Normal);
            component.SoundStream?.Stop();
            component.SoundStream = null;
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
