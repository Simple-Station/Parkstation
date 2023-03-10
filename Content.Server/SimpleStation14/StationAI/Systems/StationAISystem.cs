using Content.Shared.Actions;
using Content.Shared.EntityHealthBar;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Content.Shared.SimpleStation14.StationAI.Events;

namespace Content.Shared.SimpleStation14.StationAI
{
    public sealed class StationAISystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StationAIComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<StationAIComponent, ComponentShutdown>(OnShutdown);

            SubscribeLocalEvent<AIHealthOverlayEvent>(OnHealthOverlayEvent);
        }

        private void OnStartup(EntityUid uid, StationAIComponent component, ComponentStartup args)
        {
            if (!_prototypeManager.TryIndex(component.Action, out InstantActionPrototype? proto)) return;
            var action = new InstantAction(proto);
            _actions.AddAction(uid, action, null);
        }

        private void OnShutdown(EntityUid uid, StationAIComponent component, ComponentShutdown args)
        {
            if (!_prototypeManager.TryIndex(component.Action, out InstantActionPrototype? proto)) return;
            var action = new InstantAction(proto);
            _actions.RemoveAction(uid, action, null);
        }


        private void OnHealthOverlayEvent(AIHealthOverlayEvent args)
        {
            RaiseNetworkEvent(new NetworkedAIHealthOverlayEvent(args.Performer));
            args.Handled = true;
        }
    }
}
