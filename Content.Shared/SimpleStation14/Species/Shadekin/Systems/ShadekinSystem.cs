using Content.Shared.Examine;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Network;
using Content.Shared.IdentityManagement;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Random;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<ShadekinComponent, ComponentInit>(OnInit);

            SubscribeLocalEvent<ShadekinComponent, ComponentGetState>(GetCompState);
            SubscribeLocalEvent<ShadekinComponent, ComponentHandleState>(HandleCompState);

            // Due to duplicate subscriptions, removal of the alert on shutdown is in ShadekinDarkenSystem
        }

        private void OnExamine(EntityUid uid, ShadekinComponent component, ExaminedEvent args)
        {
            if (args.IsInDetailsRange)
            {
                var powerType = _powerSystem.GetLevelName(component.PowerLevel);

                if (args.Examined == args.Examiner)
                {
                    args.PushMarkup(Loc.GetString("shadekin-power-examined-self",
                        ("power", _powerSystem.GetLevelInt(component.PowerLevel)),
                        ("powerMax", component.PowerLevelMax),
                        ("powerType", powerType)
                    ));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("shadekin-power-examined-other",
                        ("target", Identity.Entity(uid, _entityManager)),
                        ("powerType", powerType)
                    ));
                }
            }
        }

        private void OnInit(EntityUid uid, ShadekinComponent component, ComponentInit args)
        {
            if (component.PowerLevel <= ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min] + 1f)
                _powerSystem.SetPowerLevel(component.Owner, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Okay]);

            component.ForceSwapRate = _random.NextFloat(component.ForceSwapRateMin, component.ForceSwapRateMax);
            component.TiredRate = _random.NextFloat(component.TiredRateMin, component.TiredRateMax);
        }


        private void GetCompState(EntityUid uid, ShadekinComponent component, ref ComponentGetState args)
        {
            args.State = new ShadekinComponentState
            {
                PowerLevel = component.PowerLevel,
                PowerLevelGain = component.PowerLevelGain,
                PowerLevelGainMultiplier = component.PowerLevelGainMultiplier,
                PowerLevelGainEnabled = component.PowerLevelGainEnabled,
                Blackeye = component.Blackeye
            };
        }

        private void HandleCompState(EntityUid uid, ShadekinComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not ShadekinComponentState shadekin)
            {
                return;
            }

            component.PowerLevel = shadekin.PowerLevel;
            component.PowerLevelGain = shadekin.PowerLevelGain;
            component.PowerLevelGainMultiplier = shadekin.PowerLevelGainMultiplier;
            component.PowerLevelGainEnabled = shadekin.PowerLevelGainEnabled;
            component.Blackeye = shadekin.Blackeye;
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = _entityManager.EntityQuery<ShadekinComponent>();

            // Update power level for all shadekin
            // Prediction won't work, client updates ~10x faster with the same given frameTime (~0.3333... in my testing)
            if (_net.IsServer)
            {
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
                        Dirty(component);
                    }


                    #region Random Stuff

                    #region ForceSwap
                    // Check if they're at max power
                    if (component.PowerLevel >= ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Max])
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
                            if (_entityManager.TryGetComponent<ShadekinDarkSwappedComponent>(component.Owner, out var _))
                            {
                                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(component.Owner, false));
                                _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(component.Owner);
                            }
                            else
                            {
                                RaiseNetworkEvent(new ShadekinDarkSwappedEvent(component.Owner, true));
                                _entityManager.AddComponent<ShadekinDarkSwappedComponent>(component.Owner);
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
                            ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Tired] +
                            ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Okay]
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
                            RaiseLocalEvent(new ShadekinRestEventResponse(component.Owner, true));
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

            foreach (var component in query)
            {
                _powerSystem.UpdateAlert(component.Owner, true, component.PowerLevel);
            }
        }
    }
}
