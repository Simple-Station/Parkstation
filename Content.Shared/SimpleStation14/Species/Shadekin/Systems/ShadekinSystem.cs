using Content.Shared.Examine;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Network;
using Content.Shared.IdentityManagement;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinComponent, ExaminedEvent>(OnExamine);
            SubscribeLocalEvent<ShadekinComponent, ComponentInit>(OnInit);

            SubscribeLocalEvent<ShadekinComponent, ComponentGetState>(GetCompState);
            SubscribeLocalEvent<ShadekinComponent, ComponentHandleState>(HandleCompState);

            // Due to duplicate subscriptions, removal of the alert is in ShadekinDarkenSystem
        }

        private void OnExamine(EntityUid uid, ShadekinComponent component, ExaminedEvent args)
        {
            if (args.IsInDetailsRange && !_net.IsClient)
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
        }


        private void GetCompState(EntityUid uid, ShadekinComponent component, ref ComponentGetState args)
        {
            args.State = new ShadekinComponentState
            {
                PowerLevel = component.PowerLevel,
                PowerLevelMax = component.PowerLevelMax,
                PowerLevelMin = component.PowerLevelMin,
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
            component.PowerLevelMax = shadekin.PowerLevelMax;
            component.PowerLevelMin = shadekin.PowerLevelMin;
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
            if (_net.IsServer)
            {
                foreach (var component in query)
                {
                    // These MUST be  TryUpdatePowerLevel  THEN  TryBlackeye  or else init will always blackeye
                    _powerSystem.TryUpdatePowerLevel(component.Owner, frameTime);
                    if (!component.Blackeye) _powerSystem.TryBlackeye(component.Owner);

                    component.Accumulator += frameTime;
                    if (component.Accumulator < component.AccumulatorRate) continue;
                    component.Accumulator = 0f;

                    Dirty(component);
                }
            }

            foreach (var component in query)
            {
                _powerSystem.UpdateAlert(component.Owner, true, component.PowerLevel);
            }
        }
    }
}
