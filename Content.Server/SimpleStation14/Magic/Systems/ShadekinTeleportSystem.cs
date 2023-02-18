using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.SimpleStation14.Magic.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinTeleportComponent, ComponentStartup>(Startup);
            SubscribeLocalEvent<ShadekinTeleportComponent, ComponentShutdown>(Shutdown);

            SubscribeLocalEvent<ShadekinTeleportEvent>(Teleport);
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

        private void Teleport(ShadekinTeleportEvent args)
        {
            if (args.Handled) return;

            var transform = Transform(args.Performer);
            if (transform.MapID != args.Target.GetMapId(EntityManager)) return;

            _transformSystem.SetCoordinates(args.Performer, args.Target);
            transform.AttachToGridOrMap();

            _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));

            _staminaSystem.TakeStaminaDamage(args.Performer, args.StaminaCost);

            args.Handled = true;
        }
    }
}
