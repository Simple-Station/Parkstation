using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinSystemBlackeyeTraitSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedHumanoidAppearanceSystem _sharedHumanoidAppearance = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinBlackeyeTraitComponent, ComponentStartup>(OnStartup);
        }

        private void OnStartup(EntityUid uid, ShadekinBlackeyeTraitComponent _, ComponentStartup args)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadekinDarkSwapComponent>(uid);
            _entityManager.RemoveComponent<ShadekinDarkSwappedComponent>(uid);
            _entityManager.RemoveComponent<ShadekinRestComponent>(uid);
            _entityManager.RemoveComponent<ShadekinTeleportComponent>(uid);

            // Popup
            if (_net.IsClient)
            {
                _popupSystem.PopupEntity(Loc.GetString("shadekin-blackeye"), uid, uid, PopupType.Medium);
            }

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadekinComponent>(uid, out var component))
            {
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component.Owner, ShadekinComponent.PowerThresholds[ShadekinPowerThreshold.Min]);
            }
        }
    }
}
