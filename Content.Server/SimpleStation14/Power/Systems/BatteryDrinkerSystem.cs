using System.Diagnostics.CodeAnalysis;
using Content.Server.Power.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Helpers;
using Content.Shared.PowerCell.Components;
using Content.Shared.SimpleStation14.Silicon;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Server.SimpleStation14.Silicon.Charge;
using Content.Server.Power.EntitySystems;

namespace Content.Server.SimpleStation14.Power;

public sealed class BatteryDrinkerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);

        SubscribeLocalEvent<BatteryDrinkerComponent, BatteryDrinkerEvent>(OnDoAfter);
    }

    private void AddAltVerb(EntityUid uid, BatteryComponent batteryComponent, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!EntityManager.TryGetComponent<BatteryDrinkerComponent>(args.User, out var drinkerComp) ||
            !TestDrinkableBattery(uid, drinkerComp) ||
            !TryGetFillableBattery(args.User, out var drinkerBattery))
            return;

        AlternativeVerb verb = new()
        {
            Act = () => DrinkBattery(uid, args.User, drinkerComp),
            Text = "system-battery-drinker-verb-drink",
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/drink.svg.192dpi.png")),
        };

        args.Verbs.Add(verb);
    }

    private bool TestDrinkableBattery(EntityUid target, BatteryDrinkerComponent drinkerComp)
    {
        if (!drinkerComp.DrinkAll && !HasComp<BatteryDrinkerSourceComponent>(target))
            return false;

        return true;
    }

    private bool TryGetFillableBattery(EntityUid uid, [NotNullWhen(true)] out BatteryComponent? battery)
    {
        if (_silicon.TryGetSiliconBattery(uid, out battery))
            return true;

        if (EntityManager.TryGetComponent(uid, out battery))
            return true;

        if (EntityManager.TryGetComponent<PowerCellSlotComponent>(uid, out var powerCellSlot) &&
            _slots.TryGetSlot(uid, powerCellSlot.CellSlotId, out var slot) &&
            slot.Item != null &&
            EntityManager.TryGetComponent(slot.Item.Value, out battery))
            return true;

        return false;
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryDrinkerComponent drinkerComp)
    {
        var doAfterTime = drinkerComp.DrinkSpeed;

        if (EntityManager.TryGetComponent<BatteryDrinkerSourceComponent>(target, out var sourceComp))
            doAfterTime *= sourceComp.DrinkSpeedMulti;
        else
            doAfterTime *= 2.5f;

        var args = new DoAfterArgs(user, doAfterTime, new BatteryDrinkerEvent(), user, target)
        {
            BreakOnDamage = true,
            BreakOnTargetMove = true,
            Broadcast = false,
            DistanceThreshold = 1.35f,
            RequireCanInteract = true,
            CancelDuplicate = false
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnDoAfter(EntityUid uid, BatteryDrinkerComponent drinkerComp, DoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        var source = args.Target.Value;
        var drinker = uid;
        var sourceBattery = EntityManager.GetComponent<BatteryComponent>(source);

        TryGetFillableBattery(drinker, out var drinkerBattery);

        EntityManager.TryGetComponent<BatteryDrinkerSourceComponent>(source, out var sourceComp);

        DebugTools.AssertNotNull(drinkerBattery);

        if (drinkerBattery == null)
            return;

        var amountToDrink = drinkerComp.DrinkMultiplier * 1000;

        amountToDrink = MathF.Min(amountToDrink, sourceBattery.CurrentCharge);
        amountToDrink = MathF.Min(amountToDrink, drinkerBattery.MaxCharge - drinkerBattery.CurrentCharge);

        if (sourceComp != null && sourceComp.MaxAmount > 0)
        {
            amountToDrink = MathF.Min(amountToDrink, (float) sourceComp.MaxAmount);
        }

        if (amountToDrink <= 0)
        {
            // Do empty stuff
            return;
        }

        if (_battery.TryUseCharge(source, amountToDrink, sourceBattery))
        {
            _battery.SetCharge(source, drinkerBattery.Charge + amountToDrink, sourceBattery);
        }
        else
        {
            _battery.SetCharge(drinker, sourceBattery.Charge, drinkerBattery);
            _battery.SetCharge(source, 0, sourceBattery);
        }

        var sound = drinkerComp.DrinkSound ?? sourceComp?.DrinkSound;

        if (sound != null)
            _audio.PlayPvs(sound, source);

        // if (sourceBattery.CurrentCharge > 0)  // Make use proper looping doafters when we merge Upstream.
        //     DrinkBattery(source, drinker, sourceBattery, drinkerBattery, drinkerComp);
    }
}
