using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.Interaction.Events;

namespace Content.Shared.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinDarken : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinDarkSwappedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        private void OnInteractionAttempt(EntityUid uid, ShadekinDarkSwappedComponent component, InteractionAttemptEvent args)
        {
            if (_entityManager.TryGetComponent<TransformComponent>(args.Target, out var __)
            && !_entityManager.TryGetComponent<ShadekinDarkSwappedComponent>(args.Target, out var _))
                args.Cancel();
        }
    }
}
