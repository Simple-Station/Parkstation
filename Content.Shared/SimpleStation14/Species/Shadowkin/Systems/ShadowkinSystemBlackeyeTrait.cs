using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinBlackeyeTraitSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedHumanoidAppearanceSystem _sharedHumanoidAppearance = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinBlackeyeTraitComponent, ComponentStartup>(OnStartup);
        }

        private void OnStartup(EntityUid uid, ShadowkinBlackeyeTraitComponent _, ComponentStartup args)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadowkinDarkSwapPowerComponent>(uid);
            _entityManager.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
            _entityManager.RemoveComponent<ShadowkinRestPowerComponent>(uid);
            _entityManager.RemoveComponent<ShadowkinTeleportPowerComponent>(uid);

            // Popup
            if (_net.IsClient)
            {
                _popupSystem.PopupEntity(Loc.GetString("shadowkin-blackeye"), uid, uid, PopupType.Medium);
            }

            // Stop gaining power
            if (_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var component))
            {
                component.Blackeye = true;
                component.PowerLevelGainEnabled = false;
                _powerSystem.SetPowerLevel(component.Owner, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);
            }
        }
    }
}
