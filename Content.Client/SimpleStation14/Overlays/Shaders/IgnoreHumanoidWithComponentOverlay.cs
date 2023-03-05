using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Client.GameObjects;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Overlays
{
    public sealed class IgnoreHumanoidWithComponentOverlay : Overlay
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public List<Component> ignoredComponents = new();
        public List<Component> allowAnywayComponents = new();
        private List<EntityUid> _nonvisibleList = new();

        public IgnoreHumanoidWithComponentOverlay()
        {
            IoCManager.InjectDependencies(this);
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            var spriteQuery = _entityManager.GetEntityQuery<SpriteComponent>();

            foreach (var humanoid in _entityManager.EntityQuery<HumanoidAppearanceComponent>(true))
            {
                if (_playerManager.LocalPlayer?.ControlledEntity == humanoid.Owner) continue;

                var cont = true;
                foreach (var comp in ignoredComponents)
                {
                    if (_entityManager.TryGetComponent(humanoid.Owner, comp.GetType(), out IComponent? _))
                    {
                        cont = false;
                        break;
                    }
                }
                foreach (var comp in allowAnywayComponents)
                {
                    if (_entityManager.TryGetComponent(humanoid.Owner, comp.GetType(), out IComponent? _))
                    {
                        cont = true;
                        break;
                    }
                }
                if (cont)
                {
                    Reset(humanoid.Owner);
                    continue;
                }


                if (!spriteQuery.TryGetComponent(humanoid.Owner, out var sprite)) continue;

                if (sprite.Visible && !_nonvisibleList.Contains(humanoid.Owner))
                {
                    sprite.Visible = false;
                    _nonvisibleList.Add(humanoid.Owner);
                }
            }

            foreach (var humanoid in _nonvisibleList.ToArray())
            {
                if (_entityManager.Deleted(humanoid))
                {
                    _nonvisibleList.Remove(humanoid);
                    continue;
                }
            }
        }


        public void Reset()
        {
            foreach (var humanoid in _nonvisibleList.ToArray())
            {
                _nonvisibleList.Remove(humanoid);

                if (_entityManager.TryGetComponent<SpriteComponent>(humanoid, out var sprite)) sprite.Visible = true;
            }
        }

        public void Reset(EntityUid entity)
        {
            if (_nonvisibleList.Contains(entity))
            {
                _nonvisibleList.Remove(entity);

                if (_entityManager.TryGetComponent<SpriteComponent>(entity, out var sprite)) sprite.Visible = true;
            }
        }
    }
}
