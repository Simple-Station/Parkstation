using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Client.GameObjects;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinBlackeyeSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
            SubscribeLocalEvent<ShadowkinBlackeyeTraitComponent, ComponentStartup>(OnStartup);
        }

        private void OnBlackeye(ShadowkinBlackeyeEvent ev)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(ev.Uid, out var shadowkin) ||
                !_entityManager.TryGetComponent<SpriteComponent>(ev.Uid, out var sprite) ||
                !sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index) ||
                !sprite.TryGetLayer(index, out var layer))
                return;

            sprite.LayerSetColor(index, Color.Black);
        }

        private void OnStartup(EntityUid uid, ShadowkinBlackeyeTraitComponent component, ComponentStartup args)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var shadowkin) ||
                !_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite) ||
                !sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index) ||
                !sprite.TryGetLayer(index, out var layer))
                return;

            sprite.LayerSetColor(index, Color.Black);
        }
    }
}
