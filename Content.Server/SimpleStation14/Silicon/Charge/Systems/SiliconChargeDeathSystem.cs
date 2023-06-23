using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Server.Bed.Sleep;
using Content.Shared.Bed.Sleep;
using Content.Server.Sound.Components;
using Content.Server.SimpleStation14.Silicon.Charge;
using Serilog;

namespace Content.Server.SimpleStation14.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SleepingSystem _sleep = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        _silicon.TryGetSiliconBattery(uid, out var batteryComp);

        Logger.Debug($"Silicon charge state update: {args.ChargeState}");
        Logger.Debug($"Silicon battery: {batteryComp?.CurrentCharge}");

        if (args.ChargeState == ChargeState.Dead && !siliconDeadComp.Dead)
        {
            SiliconDead(uid, siliconDeadComp, batteryComp);
        }
        else if (args.ChargeState != ChargeState.Dead && siliconDeadComp.Dead)
        {
            SiliconUnDead(uid, siliconDeadComp, batteryComp);
        }
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp)
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, batteryComp);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        EntityManager.EnsureComponent<SleepingComponent>(uid);
        RemComp<SpamEmitSoundComponent>(uid); // This is also fucking stupid, I once again hate the sleeping system.
        EntityManager.EnsureComponent<ForcedSleepingComponent>(uid);

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp)
    {
        _sleep.TryWaking(uid, null, true);

        siliconDeadComp.Dead = false;

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, batteryComp));
    }
}

/// <summary>
///     A canellable event raised when a Silicon is about to go down due to charge.
/// <summary>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, BatteryComponent? batteryComp)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// <summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent? batteryComp)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// <summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, BatteryComponent? batteryComp)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
    }
}
