using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Abilities.Psionics;
using Content.Shared.SimpleStation14.StationAI;
using Content.Server.Mind.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Server.Abilities.Psionics;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Shared.Borgs;
using Content.Server.Borgs;

namespace Content.Server.SimpleStation14.StationAI
{
    public sealed class AIEyePowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly LawsSystem _laws = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AIEyePowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<AIEyePowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<AIEyeComponent, MindRemovedMessage>(OnMindRemoved);

            SubscribeLocalEvent<AIEyePowerComponent, AIEyePowerActionEvent>(OnPowerUsed);

            SubscribeLocalEvent<StationAIComponent, MobStateChangedEvent>(OnMobStateChanged);
        }

        private void OnInit(EntityUid uid, AIEyePowerComponent component, ComponentInit args)
        {
            if (!_prototypeManager.TryIndex<InstantActionPrototype>("AIEye", out var mindswap)) return;
            if (_entityManager.TryGetComponent<AIEyeComponent>(uid, out var _)) return;

            component.EyePowerAction = new InstantAction(mindswap);
            _actions.AddAction(uid, component.EyePowerAction, null);
        }

        private void OnShutdown(EntityUid uid, AIEyePowerComponent component, ComponentShutdown args)
        {
            if (!_prototypeManager.TryIndex<InstantActionPrototype>("AIEye", out var mindswap)) return;
            if (_entityManager.TryGetComponent<AIEyeComponent>(uid, out var _)) return;

            _actions.RemoveAction(uid, new InstantAction(mindswap), null);
        }

        private void OnPowerUsed(EntityUid uid, AIEyePowerComponent component, AIEyePowerActionEvent args)
        {
            var ai = _entityManager.EnsureComponent<StationAIComponent>(uid);

            // Mindswap
            var projection = Spawn(component.Prototype, Transform(uid).Coordinates);
            ai.ActiveEye = projection;
            var core = _entityManager.GetComponent<MetaDataComponent>(uid);

            Transform(projection).AttachToGridOrMap();
            _mindSwap.Swap(uid, projection);

            // Consistent name
            if (core.EntityName != "") _entityManager.GetComponent<MetaDataComponent>(projection).EntityName = core.EntityName;
            else _entityManager.GetComponent<MetaDataComponent>(projection).EntityName = "Invalid AI";

            // Consistent laws
            var laws = _entityManager.GetComponent<LawsComponent>(uid);
            foreach (var law in laws.Laws) _laws.AddLaw(projection, law);

            args.Handled = true;
        }

        private void OnMindRemoved(EntityUid uid, AIEyeComponent component, MindRemovedMessage args)
        {
            QueueDel(uid);
        }

        private void OnMobStateChanged(EntityUid uid, StationAIComponent component, MobStateChangedEvent args)
        {
            if (!_mobState.IsDead(uid)) return;

            if (component.ActiveEye != EntityUid.Invalid) _mindSwap.Swap(component.ActiveEye, uid);

            SoundSystem.Play("/Audio/SimpleStation14/Machines/AI/borg_death.ogg", Filter.Pvs(uid), uid);
        }
    }

    public sealed class AIEyePowerActionEvent : InstantActionEvent
    {

    }
}
