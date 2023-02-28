using Content.Shared.EntityHealthBar;
using Content.Shared.SimpleStation14.StationAI.Events;

namespace Content.Client.SimpleStation14.StationAI
{
    public sealed class StationAISystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<NetworkedAIHealthOverlayEvent>(OnHealthOverlayEvent);
        }

        private void OnHealthOverlayEvent(NetworkedAIHealthOverlayEvent args)
        {
            var uid = args.Performer;

            if (!_entityManager.TryGetComponent<ShowHealthBarsComponent>(uid, out var health))
            {
                health = _entityManager.AddComponent<ShowHealthBarsComponent>(uid);
            }
            else {
                _entityManager.RemoveComponent<ShowHealthBarsComponent>(uid);
            }
        }
    }
}
