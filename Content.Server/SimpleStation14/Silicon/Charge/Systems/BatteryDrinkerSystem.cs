using System.Diagnostics.CodeAnalysis;
using Content.Server.Power.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Helpers;
using Content.Shared.PowerCell.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.Silicon.Charge;

public sealed class BatteryDrinkerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);

        SubscribeLocalEvent<BatteryComponent, DoAfterEvent>(OnDoAfter);
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
            Act = () => DrinkBattery(uid, args.User, batteryComponent, drinkerBattery, drinkerComp),
            Text = "system-battery-drinker-verb-drink",
            Icon = new SpriteSpecifier.Texture(new ResourcePath("/Textures/Interface/VerbIcons/drink.svg.192dpi.png")),
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
        battery = null;

        if (EntityManager.TryGetComponent<BatteryComponent>(uid, out battery))
            return true;

        if (EntityManager.TryGetComponent<PowerCellSlotComponent>(uid, out var powerCellSlot) &&
            _slots.TryGetSlot(uid, powerCellSlot.CellSlotId, out var slot) &&
            slot.Item != null &&
            EntityManager.TryGetComponent<BatteryComponent>(slot.Item.Value, out battery))
            return true;

        return false;
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryComponent batteryComp, BatteryComponent drinkerBatteryComp, BatteryDrinkerComponent drinkerComp)
    {
        var doAfterTime = drinkerComp.DrinkSpeed;

        if (EntityManager.TryGetComponent<BatteryDrinkerSourceComponent>(target, out var sourceComp))
            doAfterTime *= sourceComp.DrinkSpeedMulti;
        else
            doAfterTime *= 2.5f;

        _doAfter.DoAfter(new DoAfterEventArgs(user, doAfterTime, target:target)
        {
            RaiseOnTarget = true,
            RaiseOnUser = false,
            BreakOnUserMove = false,
            BreakOnDamage = true,
            BreakOnStun = true,
            BreakOnTargetMove = false,
            MovementThreshold = 0.25f,
            PostCheck = () => user.InRangeUnOccluded(target, 1.5f)
        });
    }

    private void OnDoAfter(EntityUid uid, BatteryComponent batteryComponent, DoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var source = uid;
        var drinker = args.Args.User;
        var drinkerComp = EntityManager.GetComponent<BatteryDrinkerComponent>(drinker);
        var sourceBattery = EntityManager.GetComponent<BatteryComponent>(source);

        TryGetFillableBattery(drinker, out var drinkerBattery);

        EntityManager.TryGetComponent<BatteryDrinkerSourceComponent>(source, out var sourceComp);

        DebugTools.AssertNotNull(drinkerBattery);

        if (drinkerBattery == null)
            return;

        var amountToDrink = drinkerComp.DrinkMultiplier * 1000;

        amountToDrink = MathF.Min(amountToDrink, batteryComponent.CurrentCharge);
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

        if (sourceBattery.TryUseCharge(amountToDrink))
        {
            drinkerBattery.CurrentCharge += amountToDrink;
        }
        else
        {
            drinkerBattery.CurrentCharge += sourceBattery.CurrentCharge;
            sourceBattery.CurrentCharge = 0;
        }

        var sound = drinkerComp.DrinkSound ?? sourceComp?.DrinkSound;

        // Log sound
        Logger.InfoS("battery", $"User {drinker} drank {amountToDrink} from {source}.");
        Logger.Info("Sound to play: " + sound?.ToString() ?? "null");

        if (sound != null)
            _audio.PlayPvs(sound, source);

        if (sourceBattery.CurrentCharge > 0)
            DrinkBattery(source, drinker, sourceBattery, drinkerBattery, drinkerComp);
    }
}
