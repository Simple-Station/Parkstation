using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedInteractionSystem _interact = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<ShadowkinComponent, ComponentInit>(OnInit);
        }

        private void OnExamine(EntityUid uid, ShadowkinComponent component, ExaminedEvent args)
        {
            if (args.IsInDetailsRange)
            {
                var powerType = _powerSystem.GetLevelName(component.PowerLevel);

                if (args.Examined == args.Examiner)
                {
                    args.PushMarkup(Loc.GetString("shadowkin-power-examined-self",
                        ("power", (int) component.PowerLevel),
                        ("powerMax", component.PowerLevelMax),
                        ("powerType", powerType)
                    ));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("shadowkin-power-examined-other",
                        ("target", Identity.Entity(uid, _entityManager)),
                        ("powerType", powerType)
                    ));
                }
            }
        }

        private void OnInit(EntityUid uid, ShadowkinComponent component, ComponentInit args)
        {
            if (component.PowerLevel <= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min] + 1f)
                _powerSystem.SetPowerLevel(uid, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Okay]);

            component.MaxedPowerAccumulator = _random.NextFloat(component.MaxedPowerRateMin, component.MaxedPowerRateMax);
            component.MinPowerAccumulator = _random.NextFloat(component.MinRateMin, component.MinRateMax);
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = _entityManager.EntityQuery<ShadowkinComponent>();

            // Update power level for all shadowkin
            foreach (var component in query)
            {
                var oldPowerLevel = _powerSystem.GetLevelName(component.PowerLevel);

                if (!component.Blackeye)
                    _powerSystem.TryBlackeye(component.Owner);
                _powerSystem.TryUpdatePowerLevel(component.Owner, frameTime);

                if (oldPowerLevel != _powerSystem.GetLevelName(component.PowerLevel))
                {
                    _powerSystem.UpdateAlert(component.Owner, true, component.PowerLevel);
                    Dirty(component);
                }

                #region MaxPower
                // Check if they're at max power
                if (component.PowerLevel >= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Max])
                {
                    // If so, start the timer
                    component.MaxedPowerAccumulator -= frameTime;

                    // If the time's not up, return.
                    if (component.MaxedPowerAccumulator > 0f)
                        continue;

                    // Randomize the timer
                    component.MaxedPowerAccumulator = _random.NextFloat(component.MaxedPowerRateMin, component.MaxedPowerRateMax);

                    var chance = _random.Next(7);

                    if (chance <= 2)
                    {
                        ForceDarkswap(component.Owner, component);
                    }
                    else if (chance <= 7)
                    {
                        ForceTeleport(component.Owner, component);
                    }
                }
                else
                {
                    // Slowly regenerate if not maxed
                    component.MaxedPowerAccumulator += frameTime / 5f;
                    component.MaxedPowerAccumulator = Math.Clamp(component.MaxedPowerAccumulator, 0f, component.MaxedPowerRateMax);
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
                    component.MinPowerAccumulator += frameTime;

                    // If the timer is up, force rest
                    if (!(component.MinPowerAccumulator < 0f))
                        continue;

                    // Random new timer
                    component.MinPowerAccumulator = _random.NextFloat(component.MinRateMin, component.MinRateMax);

                    // Send event to rest
                    RaiseLocalEvent(new ShadowkinRestEventResponse(component.Owner, true));
                }
                else
                {
                    // Slowly regenerate if not tired
                    component.MinPowerAccumulator -= frameTime / 5f;
                    component.MinPowerAccumulator = Math.Clamp(component.MinPowerAccumulator, 0f, component.MinRateMax);
                }
                #endregion
            }
        }

        private void ForceDarkswap(EntityUid uid, ShadowkinComponent component)
        {
            // Add/Remove DarkSwapped component, which will handle the rest
            if (_entityManager.TryGetComponent<ShadowkinDarkSwappedComponent>(uid, out var _))
            {
                RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, false));
                _entityManager.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
            }
            else
            {
                RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(uid, true));
                _entityManager.AddComponent<ShadowkinDarkSwappedComponent>(uid);
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

                if (_interact.InRangeUnobstructed(uid, target.Value, 0,
                        CollisionGroup.MobMask | CollisionGroup.MobLayer))
                    break;

                target = null;
            }

            // If we didn't find a valid location, return.
            if (target == null)
                return;

            args.Target = target.Value;

            // Raise the event to teleport the Shadowkin.
            RaiseLocalEvent(args);
        }
    }
}
