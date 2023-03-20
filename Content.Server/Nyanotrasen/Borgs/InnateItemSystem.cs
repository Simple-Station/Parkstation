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
                RefreshItems(uid, component.AfterInteract);

            component.AlreadyInitialized = true;
        }

        private void RefreshItems(EntityUid uid, bool AfterInteract)
        {
            if (!TryComp<ItemSlotsComponent>(uid, out var slotsComp))
                return;

            foreach (var slot in slotsComp.Slots.Values)
            {
                Logger.Error(slot.ContainerSlot?.ContainedEntity.ToString() ?? "null");

                if (slot.ContainerSlot?.ContainedEntity is not { Valid: true } sourceItem)
                    continue;
                if (_tagSystem.HasTag(sourceItem, "NoAction"))
                    continue;

                if (AfterInteract) _actionsSystem.AddAction(uid, CreateAfterInteractAction(sourceItem), uid);
                else _actionsSystem.AddAction(uid, CreateBeforeInteractAction(sourceItem), uid);

                Logger.Error("Added action for " + sourceItem.ToString() + " to " + uid.ToString() + ".");
            }
        }

        private EntityTargetAction CreateAfterInteractAction(EntityUid uid)
        {
            EntityTargetAction action = new()
            {
                DisplayName = MetaData(uid).EntityName,
                Description = MetaData(uid).EntityDescription,
                EntityIcon = uid,
                Event = new InnateAfterInteractActionEvent(uid),
            };

            return action;
        }

        private EntityTargetAction CreateBeforeInteractAction(EntityUid uid)
        {
            EntityTargetAction action = new()
            {
                DisplayName = MetaData(uid).EntityName,
                Description = MetaData(uid).EntityDescription,
                EntityIcon = uid,
                Event = new InnateBeforeInteractActionEvent(uid),
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
