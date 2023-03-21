using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

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
                        ("power", _powerSystem.GetLevelInt(component.PowerLevel)),
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

            var query = _entityManager.EntityQuery<ShadowkinComponent>(true);

            // Update power level for all shadowkin
            foreach (var component in query)
            {
                // These MUST be  TryUpdatePowerLevel  THEN  TryBlackeye  or else init will always blackeye
                _powerSystem.TryUpdatePowerLevel(component.Owner, frameTime);
                if (!component.Blackeye) _powerSystem.TryBlackeye(component.Owner);


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

                    // If the timer is up, force swap
                    if (component.ForceSwapAccumulator > component.ForceSwapRate)
                    {
                        // Reset timer
                        component.ForceSwapAccumulator = 0f;
                        // Random new timer
                        component.ForceSwapRate = _random.NextFloat(component.ForceSwapRateMin, component.ForceSwapRateMax);

                        // Add/Remove DarkSwapped component, which will handle the rest
                        if (_entityManager.TryGetComponent<ShadowkinDarkSwappedComponent>(component.Owner, out var _))
                        {
                            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(component.Owner, false));
                            _entityManager.RemoveComponent<ShadowkinDarkSwappedComponent>(component.Owner);
                        }
                        else
                        {
                            RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(component.Owner, true));
                            _entityManager.AddComponent<ShadowkinDarkSwappedComponent>(component.Owner);
                        }
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
    }
}
