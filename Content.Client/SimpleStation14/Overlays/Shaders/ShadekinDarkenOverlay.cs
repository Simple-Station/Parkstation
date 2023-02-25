using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Client.GameObjects;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Overlays
{
    public sealed class ShadekinDarkenOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private List<EntityUid> _nonvisibleList = new();

        public override bool RequestScreenTexture => true;
        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        private readonly ShaderInstance _greyscaleShader;

        public ShadekinDarkenOverlay()
        {
            IoCManager.InjectDependencies(this);
            _greyscaleShader = _prototypeManager.Index<ShaderPrototype>("GreyscaleFullscreen").InstanceUnique();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            // Ignore non-ethereal humanoids
            var etherealQuery = _entityManager.GetEntityQuery<ShadekinComponent>();
            var spriteQuery = _entityManager.GetEntityQuery<SpriteComponent>();
            var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();

            foreach (var humanoid in _entityManager.EntityQuery<HumanoidAppearanceComponent>(true))
            {
                if (etherealQuery.TryGetComponent(humanoid.Owner, out var ethereal)) continue;
                if (!spriteQuery.TryGetComponent(humanoid.Owner, out var sprite)) continue;
                if (!xformQuery.TryGetComponent(humanoid.Owner, out var xform)) continue;

                if (sprite.Visible && !_nonvisibleList.Contains(humanoid.Owner))
                {
                    sprite.Visible = false;
                    _nonvisibleList.Add(humanoid.Owner);
                }
            }

            foreach (var humanoid in _nonvisibleList)
            {
                if (_entityManager.Deleted(humanoid))
                {
                    _nonvisibleList.Remove(humanoid);
                    continue;
                }
            }
            // Ignore non-ethereal humanoids

            // Greyscale
            if (ScreenTexture == null) return;
            if (_playerManager.LocalPlayer?.ControlledEntity is not { Valid: true } player) return;
            if (!_entityManager.HasComponent<ShadekinDarkSwapComponent>(player)) return;

            _greyscaleShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);


            var worldHandle = args.WorldHandle;
            var viewport = args.WorldBounds;
            worldHandle.SetTransform(Matrix3.Identity);
            worldHandle.UseShader(_greyscaleShader);
            worldHandle.DrawRect(viewport, Color.White);
            worldHandle.UseShader(null);
            // Greyscale
        }


        // Un-Ignore non-ethereal humanoids
        public void Reset()
        {
            foreach (var humanoid in _nonvisibleList.ToArray())
            {
                _nonvisibleList.Remove(humanoid);

                if (_entityManager.TryGetComponent<SpriteComponent>(humanoid, out var sprite)) sprite.Visible = true;
            }
        }
    }
}
