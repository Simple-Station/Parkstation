using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinBlackeyeSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
        }

        private void OnBlackeye(ShadowkinBlackeyeEvent ev)
        {
            // Remove powers
            _entityManager.RemoveComponent<ShadowkinDarkSwapPowerComponent>(ev.Uid);
            _entityManager.RemoveComponent<ShadowkinDarkSwappedComponent>(ev.Uid);
            _entityManager.RemoveComponent<ShadowkinRestPowerComponent>(ev.Uid);
            _entityManager.RemoveComponent<ShadowkinTeleportPowerComponent>(ev.Uid);
        }
    }
}
