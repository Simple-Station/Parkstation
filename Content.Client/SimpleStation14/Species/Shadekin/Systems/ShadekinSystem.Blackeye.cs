using Content.Client.Popups;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Systems;

namespace Content.Client.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystemBlackeyeSystem : EntitySystem
    {
        ShadekinSystemPowerSystem _powerSystem = new();
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<ShadekinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadekinBlackeyeEvent ev)
        {
            _entityManager.RemoveComponent<ShadekinDarkSwapComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(ev.Euid);
            _entityManager.RemoveComponent<ShadekinTeleportComponent>(ev.Euid);

            var component = _entityManager.GetComponent<ShadekinComponent>(ev.Euid);

            component.PowerLevelGainEnabled = false;
            _powerSystem.SetPowerLevel(component, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min]);

            _popupSystem.PopupEntity(Loc.GetString("shadekin-blackeye"), ev.Euid, ev.Euid, PopupType.Large);
        }
    }
}
