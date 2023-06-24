using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Server.Bed.Sleep;
using Content.Shared.Bed.Sleep;
using Content.Server.Sound.Components;
using Content.Server.SimpleStation14.Silicon.Charge;

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
        _silicon.TryGetSiliconBattery(uid, out var batteryComp, out var batteryUid);

        if (args.ChargeState == ChargeState.Dead && !siliconDeadComp.Dead)
            SiliconDead(uid, siliconDeadComp, batteryComp, batteryUid);
        else if (args.ChargeState != ChargeState.Dead && siliconDeadComp.Dead)
            SiliconUnDead(uid, siliconDeadComp, batteryComp, batteryUid);
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, batteryComp, batteryUid);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        EntityManager.EnsureComponent<SleepingComponent>(uid);
        RemComp<SpamEmitSoundComponent>(uid); // This is also fucking stupid, I once again hate the sleeping system.
        EntityManager.EnsureComponent<ForcedSleepingComponent>(uid);

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp, batteryUid));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        _sleep.TryWaking(uid, null, true);

        siliconDeadComp.Dead = false;

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, batteryComp, batteryUid));
    }
}

/// <summary>
///     A cancellable event raised when a Silicon is about to go down due to charge.
/// </summary>
/// <remarks>
///     This probably shouldn't be modified unless you intend to fill the Silicon's battery,
///     as otherwise it'll just be triggered again next frame.
/// </remarks>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// </summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// </summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}
