using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinTintSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private ColorTintOverlay _overlay = default!;

        public override void Initialize()
        {
            base.Initialize();

            _overlay = new();
            _overlay.tintColor = new(0.5f, 0f, 0.5f);
            _overlay.tintAmount = 0.25f;
            _overlay.comp = new ShadekinComponent();

            SubscribeLocalEvent<ShadekinComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShadekinComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<ShadekinComponent, PlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ShadekinComponent, PlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var uid = _player.LocalPlayer?.ControlledEntity;
            if (uid == null) return;

            if (!_entityManager.TryGetComponent(uid, out ShadekinComponent? comp)) return;

            if (_entityManager.TryGetComponent(uid, out SpriteComponent? sprite))
            {
                if (sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index))
                {
                    if (sprite.TryGetLayer(index, out var layer))
                    {
                        UpdateShader(new Vector3(layer.Color.R, layer.Color.G, layer.Color.B), comp.TintIntensity);
                    }
                }
            }
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


        private void UpdateShader(Vector3? color, float? intensity)
        {
            while (_overlayMan.HasOverlay<ColorTintOverlay>()) _overlayMan.RemoveOverlay(_overlay);

            if (color != null) _overlay.tintColor = color;
            if (intensity != null) _overlay.tintAmount = intensity;

            _overlayMan.AddOverlay(_overlay);
        }
    }
}
