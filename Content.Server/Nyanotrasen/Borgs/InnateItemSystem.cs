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

            int priority = component.StartingPriority ?? 0;
            foreach (var slot in slotsComp.Slots.Values)
            {
                if (slot.ContainerSlot?.ContainedEntity is not { Valid: true } sourceItem)
                    continue;
                if (_tagSystem.HasTag(sourceItem, "NoAction"))
                    continue;

                if (component.AfterInteract) _actionsSystem.AddAction(uid, CreateAfterInteractAction(sourceItem, priority), uid);
                else _actionsSystem.AddAction(uid, CreateBeforeInteractAction(sourceItem, priority), uid);

                priority--;
            }
        }

        private EntityTargetAction CreateAfterInteractAction(EntityUid uid, int priority)
        {
            EntityTargetAction action = new()
            {
                DisplayName = MetaData(uid).EntityName,
                Description = MetaData(uid).EntityDescription,
                EntityIcon = uid,
                Event = new InnateAfterInteractActionEvent(uid),
                Priority = priority,
            };

            return action;
        }

        private EntityTargetAction CreateBeforeInteractAction(EntityUid uid, int priority)
        {
            EntityTargetAction action = new()
            {
                DisplayName = MetaData(uid).EntityName,
                Description = MetaData(uid).EntityDescription,
                EntityIcon = uid,
                Event = new InnateBeforeInteractActionEvent(uid),
                Priority = priority,
                CheckCanAccess = false,
                Range = 25f,
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
