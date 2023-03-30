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

            SubscribeLocalEvent<ShadowkinComponent, ComponentInit>(OnInit);
        }

        private void OnBlackeye(ShadowkinBlackeyeEvent ev)
        {
            SetColor(ev.Uid, Color.Black);
        }

        private void OnStartup(EntityUid uid, ShadowkinBlackeyeTraitComponent component, ComponentStartup args)
        {
            SetColor(uid, Color.Black);

            RaiseNetworkEvent(new ShadowkinBlackeyeEvent(uid, false));
        }


        private void OnInit(EntityUid uid, ShadowkinComponent component, ComponentInit args)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out _) ||
                !_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite) ||
                !sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index) ||
                !sprite.TryGetLayer(index, out var layer))
                return;

            if (layer.Color != Color.Black)
                return;

            RaiseNetworkEvent(new ShadowkinBlackeyeEvent(uid, false));
        }


        private void SetColor(EntityUid uid, Color color)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out _) ||
                !_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite) ||
                !sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index) ||
                !sprite.TryGetLayer(index, out var layer))
                return;

            sprite.LayerSetColor(index, color);
        }
    }
}
