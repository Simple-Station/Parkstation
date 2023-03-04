using Content.Shared.Interaction.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinDarken : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private InstantAction action = default!;

        public override void Initialize()
        {
            base.Initialize();

            action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("ShadekinDarkSwap"));

            SubscribeLocalEvent<ShadekinDarkSwapComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinDarkSwapComponent, ComponentShutdown>(Shutdown);

            SubscribeLocalEvent<ShadekinDarkSwappedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        private void Startup(EntityUid uid, ShadekinDarkSwapComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadekinDarkSwapComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, action);
        }

        private void OnInteractionAttempt(EntityUid uid, ShadekinDarkSwappedComponent component, InteractionAttemptEvent args)
        {
            if (_entityManager.TryGetComponent<TransformComponent>(args.Target, out var __)
            && !_entityManager.TryGetComponent<ShadekinDarkSwappedComponent>(args.Target, out var _))
                args.Cancel();
        }
    }
}
