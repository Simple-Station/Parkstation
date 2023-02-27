using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.SimpleStation14.Overlays
{
    /// <summary>
    ///     A simple overlay that applies a color tint to the screen.
    /// </summary>
    public sealed class ColorTintOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] IEntityManager _entityManager = default!;

        public override bool RequestScreenTexture => true;
        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        private readonly ShaderInstance _shader;

        /// <summary>
        ///     The color to tint the screen to as RGB on a scale of 0-1.
        /// </summary>
        public Vector3? tintColor = null;
        /// <summary>
        ///     The percent to tint the screen by on a scale of 0-1.
        /// </summary>
        public float? tintAmount = null;
        public Component? comp = null;

        public ColorTintOverlay()
        {
            IoCManager.InjectDependencies(this);

            _shader = _prototypeManager.Index<ShaderPrototype>("ColorTint").InstanceUnique();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null) return;
            if (_playerManager.LocalPlayer?.ControlledEntity is not { Valid: true } player) return;
            if (comp != null && !_entityManager.TryGetComponent(player, comp.GetType(), out IComponent? _)) return;

            _shader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            if (tintColor != null) _shader?.SetParameter("tint_color", (Vector3) tintColor);
            if (tintAmount != null) _shader?.SetParameter("tint_amount", (float) tintAmount);

            var worldHandle = args.WorldHandle;
            var viewport = args.WorldBounds;
            worldHandle.SetTransform(Matrix3.Identity);
            worldHandle.UseShader(_shader);
            worldHandle.DrawRect(viewport, Color.White);
            worldHandle.UseShader(null);
        }
    }
}
