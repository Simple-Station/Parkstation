using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private WorldTargetAction action = default!;

        public override void Initialize()
        {
            base.Initialize();

            action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));

            SubscribeLocalEvent<ShadekinTeleportPowerComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinTeleportPowerComponent, ComponentShutdown>(Shutdown);
        }

        private void Startup(EntityUid uid, ShadekinTeleportPowerComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadekinTeleportPowerComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, action);
        }
    }
}
