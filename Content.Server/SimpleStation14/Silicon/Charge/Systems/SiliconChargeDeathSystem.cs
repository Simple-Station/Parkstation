using Content.Server.Power.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Robust.Shared.Utility;
using Content.Shared.Stunnable;
using Content.Shared.StatusEffect;
using Content.Server.Bed.Sleep;
using Content.Shared.Bed.Sleep;
using Robust.Shared.Audio;
using Content.Server.Sound.Components;

namespace Content.Server.SimpleStation14.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    // [Dependency] private readonly IRobustRandom _random = default!;
    // [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    // [Dependency] private readonly PopupSystem _popup = default!;
    // [Dependency] private readonly IGameTiming _gameTiming = default!;
    // [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly SleepingSystem _sleepSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        EntityManager.TryGetComponent<BatteryComponent>(uid, out var batteryComp);
        EntityManager.TryGetComponent<SiliconComponent>(uid, out var siliconComp);

        DebugTools.AssertNotNull(batteryComp);
        DebugTools.AssertNotNull(siliconComp);

        if (batteryComp == null || siliconComp == null)
            return;

        if (args.ChargeState == ChargeState.Dead && !siliconDeadComp.Dead)
        {
            SiliconDead(uid, siliconDeadComp, batteryComp);
        }
        else if (args.ChargeState != ChargeState.Dead && siliconDeadComp.Dead)
        {
            SiliconUnDead(uid, siliconDeadComp, batteryComp);
        }
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent batteryComp)
    {
        var deadEvent = new SiliconChargeDeadEvent(uid, batteryComp);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        var sleepComp = EntityManager.EnsureComponent<SleepingComponent>(uid);
        RemComp<SpamEmitSoundComponent>(uid); // This is also fucking stupid, I once again hate the sleeping system.
        EntityManager.EnsureComponent<ForcedSleepingComponent>(uid);

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent batteryComp)
    {
        _sleepSystem.TryWaking(uid, null, true);

        siliconDeadComp.Dead = false;
    }
}



public sealed class SiliconChargeDeadEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent BatteryComp { get; }

    public SiliconChargeDeadEvent(EntityUid siliconUid, BatteryComponent batteryComp)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
    }
}

public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent BatteryComp { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent batteryComp)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
    }
}
