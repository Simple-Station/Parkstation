using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinTeleportEvent>(Teleport);
        }

        private void Teleport(ShadekinTeleportEvent args)
        {
            if (args.Handled) return;

            var transform = Transform(args.Performer);
            if (transform.MapID != args.Target.GetMapId(EntityManager)) return;

            _transformSystem.SetCoordinates(args.Performer, args.Target);
            transform.AttachToGridOrMap();

            _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));

            _staminaSystem.TakeStaminaDamage(args.Performer, args.PowerCost);

            args.Handled = true;
        }
    }
}
