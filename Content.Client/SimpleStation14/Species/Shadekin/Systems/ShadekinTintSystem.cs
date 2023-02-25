using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;

namespace Content.Client.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinTintSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private ShadekinTintOverlay _overlay = default!;

        public override void Initialize()
        {
            base.Initialize();

            _overlay = new();

            SubscribeLocalEvent<ShadekinComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShadekinComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<ShadekinComponent, PlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ShadekinComponent, PlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }

        private void OnStartup(EntityUid uid, ShadekinComponent component, ComponentStartup args)
        {
            if (_player.LocalPlayer?.ControlledEntity != uid) return;

            _overlayMan.AddOverlay(_overlay);
        }

        private void OnShutdown(EntityUid uid, ShadekinComponent component, ComponentShutdown args)
        {
            if (_player.LocalPlayer?.ControlledEntity != uid) return;

            _overlayMan.RemoveOverlay(_overlay);
        }

        private void OnPlayerAttached(EntityUid uid, ShadekinComponent component, PlayerAttachedEvent args)
        {
            _overlayMan.AddOverlay(_overlay);
        }

        private void OnPlayerDetached(EntityUid uid, ShadekinComponent component, PlayerDetachedEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
