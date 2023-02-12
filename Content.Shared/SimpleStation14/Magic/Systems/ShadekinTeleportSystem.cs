using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.SimpleStation14.Magic.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShadekinTeleportComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinTeleportComponent, ComponentShutdown>(Shutdown);
        }

        private void Startup(EntityUid uid, ShadekinTeleportComponent component, ComponentStartup args)
        {
            var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadekinTeleportComponent component, ComponentShutdown args)
        {
            var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
            _actionsSystem.RemoveAction(uid, action);
        }
    }
}
