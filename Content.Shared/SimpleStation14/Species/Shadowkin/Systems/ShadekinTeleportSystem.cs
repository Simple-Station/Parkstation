using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private WorldTargetAction action = default!;

        public override void Initialize()
        {
            base.Initialize();

            action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadowkinTeleport"));

            SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentShutdown>(Shutdown);
        }

        private void Startup(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, action, uid);
        }

        private void Shutdown(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, action);
        }
    }
}
