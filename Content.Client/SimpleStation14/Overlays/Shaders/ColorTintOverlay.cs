using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Shared.SimpleStation14.Traits;

namespace Content.Client.SimpleStation14.Overlays
{
    public sealed class ColorTintOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] IEntityManager _entityManager = default!;


        public override bool RequestScreenTexture => true;
        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        private readonly ShaderInstance _greyscaleShader;

        public ColorTintOverlay()
        {
            IoCManager.InjectDependencies(this);
            _greyscaleShader = _prototypeManager.Index<ShaderPrototype>("ColorTint").InstanceUnique();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null) return;
            if (_playerManager.LocalPlayer?.ControlledEntity is not { Valid: true } player) return;
            if (!_entityManager.TryGetComponent<ColorTintComponent>(player, out var component)) return;

            _greyscaleShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _greyscaleShader?.SetParameter("tint", new Vector3(component.Color));

            var worldHandle = args.WorldHandle;
            var viewport = args.WorldBounds;
            worldHandle.SetTransform(Matrix3.Identity);
            worldHandle.UseShader(_greyscaleShader);
            worldHandle.DrawRect(viewport, Color.White);
            worldHandle.UseShader(null);
        }
    }
}
