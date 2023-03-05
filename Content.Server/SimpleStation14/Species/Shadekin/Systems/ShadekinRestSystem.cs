using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Systems;

namespace Content.Server.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinRestSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinRestComponent, ShadekinRestEvent>(Rest);
        }

        private void Rest(EntityUid uid, ShadekinRestComponent component, ShadekinRestEvent args)
        {
            if (!_entityManager.TryGetComponent<ShadekinComponent>(uid, out var shadekin)) return;

            component.IsResting = !component.IsResting;
            RaiseLocalEvent(new ShadekinRestEventResponse(args.Performer, component.IsResting));

            args.Handled = true;
        }
    }
}
