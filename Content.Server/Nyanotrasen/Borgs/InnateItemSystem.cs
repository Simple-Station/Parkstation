using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Server.Mind.Components;
using Content.Shared.Tag;

namespace Content.Server.Borgs
{
    public sealed class InnateItemSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InnateItemComponent, MindAddedMessage>(OnMindAdded);
            SubscribeLocalEvent<InnateItemComponent, InnateAfterInteractActionEvent>(StartAfterInteract);
            SubscribeLocalEvent<InnateItemComponent, InnateBeforeInteractActionEvent>(StartBeforeInteract);
        }

        private void OnMindAdded(EntityUid uid, InnateItemComponent component, MindAddedMessage args)
        {
            if (!component.AlreadyInitialized)
                RefreshItems(uid, component);

            component.AlreadyInitialized = true;
        }

        private void RefreshItems(EntityUid uid, InnateItemComponent component)
        {
            if (!TryComp<ItemSlotsComponent>(uid, out var slotsComp))
                return;

            var priority = component.StartingPriority ?? 0;
            foreach (var slot in slotsComp.Slots.Values)
            {
                if (slot.ContainerSlot?.ContainedEntity is not { Valid: true } sourceItem ||
                    _tagSystem.HasTag(sourceItem, "NoAction"))
                    continue;

                _actionsSystem.AddAction(uid, CreateInteractAction(sourceItem, priority), null);

                priority--;
            }
        }


        private EntityTargetAction CreateInteractAction(EntityUid uid, int priority)
        {
            EntityTargetAction action = new()
            {
                DisplayName = MetaData(uid).EntityName,
                Description = MetaData(uid).EntityDescription,
                EntityIcon = uid,
                Event = _tagSystem.HasTag(uid, "InnateItemBeforeInteract") ? new InnateBeforeInteractActionEvent(uid) : new InnateAfterInteractActionEvent(uid),
                Priority = priority,
                CheckCanAccess = false,
                Range = 25f,
                Repeat = _tagSystem.HasTag(uid, "InnateItemRepeat"),
            };

            return action;
        }

        private void StartAfterInteract(EntityUid uid, InnateItemComponent component, InnateAfterInteractActionEvent args)
        {
            var ev = new AfterInteractEvent(args.Performer, args.Item, args.Target, Transform(args.Target).Coordinates, true);
            RaiseLocalEvent(args.Item, ev, false);
        }

        private void StartBeforeInteract(EntityUid uid, InnateItemComponent component, InnateBeforeInteractActionEvent args)
        {
            var ev = new BeforeRangedInteractEvent(args.Performer, args.Item, args.Target, Transform(args.Target).Coordinates, true);
            RaiseLocalEvent(args.Item, ev, false);
        }
    }

    public sealed class InnateAfterInteractActionEvent : EntityTargetActionEvent
    {
        public EntityUid Item;

        public InnateAfterInteractActionEvent(EntityUid item)
        {
            Item = item;
        }
    }

    public sealed class InnateBeforeInteractActionEvent : EntityTargetActionEvent
    {
        public EntityUid Item;

        public InnateBeforeInteractActionEvent(EntityUid item)
        {
            Item = item;
        }
    }
}
