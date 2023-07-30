using Content.Server.GameTicking.Rules.Components;
using Content.Server.Radio;
using Robust.Shared.Random;
using Content.Server.Light.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Radio.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.StatusEffect; // Parkstation-Ipc
using Content.Shared.SimpleStation14.Silicon.Systems; // Parkstation-Ipc

namespace Content.Server.StationEvents.Events;

public sealed class SolarFlareRule : StationEventSystem<SolarFlareRuleComponent>
{
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!; // Parkstation-Ipc

    private float _effectTimer = 0;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioSendAttempt);
    }

    protected override void ActiveTick(EntityUid uid, SolarFlareRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        _effectTimer -= frameTime;
        if (_effectTimer < 0)
        {
            _effectTimer += 1;
            var lightQuery = EntityQueryEnumerator<PoweredLightComponent>();
            while (lightQuery.MoveNext(out var lightEnt, out var light))
            {
                if (RobustRandom.Prob(component.LightBreakChancePerSecond))
                    _poweredLight.TryDestroyBulb(lightEnt, light);
            }
            var airlockQuery = EntityQueryEnumerator<AirlockComponent, DoorComponent>();
            while (airlockQuery.MoveNext(out var airlockEnt, out var airlock, out var door))
            {
                if (airlock.AutoClose && RobustRandom.Prob(component.DoorToggleChancePerSecond))
                    _door.TryToggleDoor(airlockEnt, door);
            }
            // Parkstation-Ipc-Start // Makes Silicons respond to Solar Flares.
            var siliconQuery = EntityQueryEnumerator<SiliconComponent>();
            while (siliconQuery.MoveNext(out var siliconEnt, out _))
            {
                if (HasComp<SeeingStaticComponent>(siliconEnt))
                    continue; // So we don't mess with any ongoing effects.

                if (RobustRandom.Prob(component.DoorToggleChancePerSecond))
                {
                    _status.TryAddStatusEffect<SeeingStaticComponent>
                    (
                        siliconEnt,
                        SeeingStaticSystem.StaticKey,
                        TimeSpan.FromSeconds(RobustRandom.NextFloat(component.SiliconStaticDuration.X, component.SiliconStaticDuration.Y)),
                        true,
                        null
                    );

                    if (TryComp<SeeingStaticComponent>(siliconEnt, out var staticComp))
                    {
                        staticComp.Multiplier = 0.80f;
                        Dirty(staticComp);
                    }
                }
            }
            // Parkstation-Ipc-End
        }
    }

    private void OnRadioSendAttempt(ref RadioReceiveAttemptEvent args)
    {
        var query = EntityQueryEnumerator<SolarFlareRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var flare, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;

            if (!flare.AffectedChannels.Contains(args.Channel.ID))
                continue;

            if (!flare.OnlyJamHeadsets || (HasComp<HeadsetComponent>(args.RadioReceiver) || HasComp<HeadsetComponent>(args.RadioSource)))
                args.Cancelled = true;
        }
    }
}
