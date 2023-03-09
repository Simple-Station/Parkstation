using Content.Shared.Interaction.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinDarken : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        private InstantAction action = default!;

        public override void Initialize()
        {
            base.Initialize();

            action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("ShadekinDarkSwap"));

            SubscribeLocalEvent<ShadekinDarkSwapPowerComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinDarkSwapPowerComponent, ComponentShutdown>(Shutdown);

            SubscribeLocalEvent<ShadekinDarkSwappedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        private void Startup(EntityUid uid, ShadekinDarkSwapPowerComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadekinDarkSwapPowerComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, action);
        }

        private void OnInteractionAttempt(EntityUid uid, ShadekinDarkSwappedComponent component, InteractionAttemptEvent args)
        {
            if (args.Target != null && _entityManager.TryGetComponent<TransformComponent>(args.Target, out var __)
            && !_entityManager.TryGetComponent<ShadekinDarkSwappedComponent>(args.Target, out var _))
            {
                args.Cancel();
                if (!_gameTiming.InPrediction)
                {
                    _popupSystem.PopupEntity(Loc.GetString("ethereal-pickup-fail"), args.Target.Value, uid, PopupType.Small);
                }
            }
        }
    }
}
