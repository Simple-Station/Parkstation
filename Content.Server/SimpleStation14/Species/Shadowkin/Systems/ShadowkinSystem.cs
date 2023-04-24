using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
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
                _powerSystem.SetPowerLevel(component.Owner, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Okay]);

            component.ForceSwapRate = _random.NextFloat(component.ForceSwapRateMin, component.ForceSwapRateMax);
            component.TiredRate = _random.NextFloat(component.TiredRateMin, component.TiredRateMax);
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = _entityManager.EntityQuery<ShadowkinComponent>();

            // Update power level for all shadowkin
            foreach (var component in query)
            {
                _powerSystem.TryUpdatePowerLevel(component.Owner, frameTime);
                if (!component.Blackeye)
                    _powerSystem.TryBlackeye(component.Owner);


                // Sync client and server
                component.DirtyAccumulator += frameTime;
                if (component.DirtyAccumulator > component.DirtyAccumulatorRate)
                {
                    component.DirtyAccumulator = 0f;

                    _powerSystem.UpdateAlert(component.Owner, true, component.PowerLevel);
                    Dirty(component);
                }


                #region Random Stuff

                #region ForceSwap
                // Check if they're at max power
                if (component.PowerLevel >= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Max])
                {
                    // If so, start the timer
                    component.ForceSwapAccumulator += frameTime;

                    // If the time's not up, return.
                    if (component.ForceSwapAccumulator < component.ForceSwapRateMax)
                        return;

                    // Reset the timer, and randomize a new one
                    component.ForceSwapAccumulator = 0f;
                    component.ForceSwapRate = _random.NextFloat(component.ForceSwapRateMin, component.ForceSwapRateMax);

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
                    // Slowly regenerate if not max power
                    component.ForceSwapAccumulator -= frameTime / 3f;
                    component.ForceSwapAccumulator = Math.Clamp(component.ForceSwapAccumulator, 0f, component.ForceSwapRate);
                }
                #endregion

                #region Tired
                // Check if they're at the average of the Tired and Okay thresholds
                // Just Tired is too little, and Okay is too much
                if (component.PowerLevel <=
                    (
                        ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Tired] +
                        ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Okay]
                    ) / 2f
                )
                {
                    // If so, start the timer
                    component.TiredAccumulator += frameTime;

                    // If the timer is up, force rest
                    if (component.TiredAccumulator > component.TiredRate)
                    {
                        // Reset timer
                        component.TiredAccumulator = 0f;
                        // Random new timer
                        component.TiredRate = _random.NextFloat(component.TiredRateMin, component.TiredRateMax);

                        // Send event to rest
                        RaiseLocalEvent(new ShadowkinRestEventResponse(component.Owner, true));
                    }
                }
                else
                {
                    // Slowly regenerate if not tired
                    component.TiredAccumulator -= frameTime / 5f;
                    component.TiredAccumulator = Math.Clamp(component.TiredAccumulator, 0f, component.TiredRate);
                }
                #endregion
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
            var args = new ShadowkinTeleportEvent();
            args.Performer = uid;

            // Pick a random location on the map until we find one that can be reached.
            var coords = Transform(uid).Coordinates;
            EntityCoordinates? target = null;

            for (var i = 8; i != 0; i--) // It'll iterate up to 8 times, shrinking in distance each time, and if it doesn't find a valid location, it'll return.
            {
                var angle = Angle.FromDegrees(_random.Next(360));
                var length = i;

                var offset = new Vector2((float) (length * Math.Cos(angle)), (float) (length * Math.Sin(angle)));

                target = coords.Offset(offset);

                if (_interact.InRangeUnobstructed(uid, target.Value, 0))
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
