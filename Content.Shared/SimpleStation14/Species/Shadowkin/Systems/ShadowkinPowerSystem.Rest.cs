using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Bed.Sleep;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinRestSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectSystem = default!;

        private InstantAction _action = default!;

        public override void Initialize()
        {
            base.Initialize();

            _action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("ShadowkinRest"));

            SubscribeLocalEvent<ShadowkinRestEventResponse>(Rest);

            SubscribeLocalEvent<ShadowkinRestPowerComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShadowkinRestPowerComponent, ComponentShutdown>(OnShutdown);
        }

        private void Rest(ShadowkinRestEventResponse args)
        {
            if (!_entityManager.TryGetComponent<ShadowkinComponent>(args.Performer, out _))
                return;
            if (!_entityManager.TryGetComponent<ShadowkinRestPowerComponent>(args.Performer, out var rest))
                return;
            rest.IsResting = args.IsResting;

            if (args.IsResting)
            {
                _statusEffectSystem.TryAddStatusEffect<ForcedSleepingComponent>(args.Performer, "ForcedSleep", TimeSpan.FromDays(1), false);

                _powerSystem.TryAddMultiplier(args.Performer);
            }
            else
            {
                _statusEffectSystem.TryRemoveStatusEffect(args.Performer, "ForcedSleep");

                _powerSystem.TryAddMultiplier(args.Performer, -1f);
            }
        }


        private void OnStartup(EntityUid uid, ShadowkinRestPowerComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, _action, uid);
        }

        private void OnShutdown(EntityUid uid, ShadowkinRestPowerComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, _action);
        }
    }
}
