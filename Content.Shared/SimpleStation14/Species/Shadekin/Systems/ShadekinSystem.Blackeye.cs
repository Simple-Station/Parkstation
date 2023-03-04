using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystemBlackeyeSystem : EntitySystem
    {
        ShadekinSystemPowerSystem _powerSystem = new();
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadekinBlackeyeEvent ev)
        {
            _entityManager.RemoveComponent<ShadekinDarkSwapComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinTeleportComponent>(ev.Euid);

            var component = _entityManager.GetComponent<ShadekinComponent>(ev.Euid);

            component.PowerLevelGainEnabled = false;
            _powerSystem.SetPowerLevel(component, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min]);
        }
    }
}
