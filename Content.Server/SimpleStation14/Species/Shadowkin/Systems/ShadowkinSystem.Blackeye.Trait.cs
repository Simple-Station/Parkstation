using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
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
            RaiseLocalEvent(uid, new ShadowkinBlackeyeEvent(uid, false));
        }
    }
}
