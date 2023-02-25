using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.SimpleStation14.Overlays
{
    public sealed class ShadekinTintOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] IEntityManager _entityManager = default!;


        public override bool RequestScreenTexture => true;
        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        private readonly ShaderInstance _tintShader;

        public ShadekinTintOverlay()
        {
            IoCManager.InjectDependencies(this);
            _tintShader = _prototypeManager.Index<ShaderPrototype>("ShadekinTint").InstanceUnique();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null) return;
            if (_playerManager.LocalPlayer?.ControlledEntity is not { Valid: true } player) return;
            if (!_entityManager.TryGetComponent(player, out ShadekinComponent? shadekin)) return;

            _tintShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _tintShader?.SetParameter("tintColor", shadekin.Tint);
            _tintShader?.SetParameter("tintIntensity", shadekin.TintIntensity);


            var worldHandle = args.WorldHandle;
            var viewport = args.WorldBounds;
            worldHandle.SetTransform(Matrix3.Identity);
            worldHandle.UseShader(_tintShader);
            worldHandle.DrawRect(viewport, Color.White);
            worldHandle.UseShader(null);
        }
    }
}
