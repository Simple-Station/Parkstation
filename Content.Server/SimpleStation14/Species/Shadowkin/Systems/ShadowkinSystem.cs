using Content.Server.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ShadowkinComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowkinComponent, ComponentShutdown>(OnShutdown);
    }


    private void OnExamine(EntityUid uid, ShadowkinComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var powerType = ShadowkinPowerSystem.GetLevelName(component.PowerLevel);

        // Show exact values for yourself
        if (args.Examined == args.Examiner)
        {
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-self",
                ("power", (int) component.PowerLevel),
                ("powerMax", component.PowerLevelMax),
                ("powerType", powerType)
            ));
        }
        // Show general values for others
        else
        {
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-other",
                ("target", Identity.Entity(uid, _entity)),
                ("powerType", powerType)
            ));
        }
    }

    private void OnInit(EntityUid uid, ShadowkinComponent component, ComponentInit args)
    {
        if (component.PowerLevel <= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min] + 1f)
            _power.SetPowerLevel(uid, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Okay]);

        var max = _random.NextFloat(component.MaxedPowerRateMin, component.MaxedPowerRateMax);
        component.MaxedPowerAccumulator = max;
        component.MaxedPowerRoof = max;

        var min = _random.NextFloat(component.MinPowerMin, component.MinPowerMax);
        component.MinPowerAccumulator = min;
        component.MinPowerRoof = min;
    }

    private void OnShutdown(EntityUid uid, ShadowkinComponent component, ComponentShutdown args)
    {
        _power.UpdateAlert(uid, false);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = _entity.EntityQueryEnumerator<ShadowkinComponent>();

        // Update power level for all shadowkin
        while (query.MoveNext(out var uid, out var component))
        {
            var oldPowerLevel = ShadowkinPowerSystem.GetLevelName(component.PowerLevel);

            _power.TryUpdatePowerLevel(uid, frameTime);

            if (oldPowerLevel != ShadowkinPowerSystem.GetLevelName(component.PowerLevel))
            {
                _power.TryBlackeye(uid);
                _power.UpdateAlert(uid, true, component.PowerLevel);
                Dirty(component);
            }

            #region MaxPower
            // Check if they're at max power
            if (component.PowerLevel >= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Max])
            {
                // If so, start the timer
                component.MaxedPowerAccumulator -= frameTime;

                // If the time's up, do things
                if (component.MaxedPowerAccumulator <= 0f)
                {
                    // Randomize the timer
                    var next = _random.NextFloat(component.MaxedPowerRateMin, component.MaxedPowerRateMax);
                    component.MaxedPowerRoof = next;
                    component.MaxedPowerAccumulator = next;

                    var chance = _random.Next(7);

                    if (chance <= 2)
                    {
                        ForceDarkSwap(uid, component);
                    }
                    else if (chance <= 7)
                    {
                        ForceTeleport(uid, component);
                    }
                }
            }
            else
            {
                // Slowly regenerate if not maxed
                component.MaxedPowerAccumulator += frameTime / 5f;
                component.MaxedPowerAccumulator = Math.Clamp(component.MaxedPowerAccumulator, 0f, component.MaxedPowerRoof);
            }
            #endregion

            #region MinPower
            // Check if they're at the average of the Tired and Okay thresholds
            // Just Tired is too little, and Okay is too much, get the average
            if (component.PowerLevel <=
                (
                    ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Tired] +
                    ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Okay]
                ) / 2f
            )
            {
                // If so, start the timer
                component.MinPowerAccumulator -= frameTime;

                // If the timer is up, force rest
                if (component.MinPowerAccumulator <= 0f)
                {
                    // Random new timer
                    var next = _random.NextFloat(component.MinPowerMin, component.MinPowerMax);
                    component.MinPowerRoof = next;
                    component.MinPowerAccumulator = next;

                    // Send event to rest
                    RaiseLocalEvent(uid, new ShadowkinRestEvent { Performer = uid });
                }
            }
            else
            {
                // Slowly regenerate if not tired
                component.MinPowerAccumulator += frameTime / 5f;
                component.MinPowerAccumulator = Math.Clamp(component.MinPowerAccumulator, 0f, component.MinPowerRoof);
            }
            #endregion
        }
    }

    private void ForceDarkSwap(EntityUid uid, ShadowkinComponent component)
    {
        // Add/Remove DarkSwapped component, which will handle the rest
        if (_entity.HasComponent<ShadowkinDarkSwappedComponent>(uid))
        {
            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, false));
            _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
        }
        else
        {
            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, true));
            _entity.EnsureComponent<ShadowkinDarkSwappedComponent>(uid);
        }
    }

    private void ForceTeleport(EntityUid uid, ShadowkinComponent component)
    {
        // Create the event we'll later raise, and set it to our Shadowkin.
        var args = new ShadowkinTeleportEvent
        {
            Performer = uid
        };

        // Pick a random location on the map until we find one that can be reached.
        var coords = Transform(uid).Coordinates;
        EntityCoordinates? target = null;

        for (var i = 8; i != 0; i--) // It'll iterate up to 8 times, shrinking in distance each time, and if it doesn't find a valid location, it'll return.
        {
            var angle = Angle.FromDegrees(_random.Next(360));
            var offset = new Vector2((float) (i * Math.Cos(angle)), (float) (i * Math.Sin(angle)));

            target = coords.Offset(offset);

            if (_interaction.InRangeUnobstructed(uid, target.Value, 0,
                    CollisionGroup.MobMask | CollisionGroup.MobLayer))
                break;

            target = null;
        }

        // If we didn't find a valid location, return.
        if (target == null)
            return;

        args.Target = target.Value;

        // Raise the event to teleport the Shadowkin.
        RaiseLocalEvent(uid, args);
    }
}
