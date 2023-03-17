
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinRestSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinRestPowerComponent, ShadowkinRestEvent>(Rest);
        }

        private void Rest(EntityUid uid, ShadowkinRestPowerComponent component, ShadowkinRestEvent args)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(uid, out var shadowkin)) return;

            component.IsResting = !component.IsResting;
            RaiseLocalEvent(new ShadowkinRestEventResponse(args.Performer, component.IsResting));

            args.Handled = true;
        }
    }
}
