using Content.Shared.Examine;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Network;
using Content.Shared.IdentityManagement;

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


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            // Update power level for all shadekin
            foreach (var component in _entityManager.EntityQuery<ShadekinComponent>())
            {
                // These MUST be  TryUpdatePowerLevel  THEN  TryBlackeye  or else init will always blackeye
                _powerSystem.TryUpdatePowerLevel(component.Owner, frameTime);
                if (!component.Blackeye) _powerSystem.TryBlackeye(component.Owner);
            }
        }
    }
}
