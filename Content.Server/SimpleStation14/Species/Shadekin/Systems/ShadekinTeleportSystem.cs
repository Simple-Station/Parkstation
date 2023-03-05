using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Content.Shared.SimpleStation14.Species.Shadekin.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinSystemPowerSystem _powerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinTeleportEvent>(Teleport);
        }

        private void Teleport(ShadekinTeleportEvent args)
        {
            if (args.Handled) return;

            if (!_entityManager.TryGetComponent<ShadekinComponent>(args.Performer, out var comp)) return;

            var transform = Transform(args.Performer);
            if (transform.MapID != args.Target.GetMapId(EntityManager)) return;

            _transformSystem.SetCoordinates(args.Performer, args.Target);
            transform.AttachToGridOrMap();

            _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));

            _powerSystem.SetPowerLevel(comp.Owner, comp.PowerLevel - args.PowerCost);

            args.Handled = true;
        }
    }
}
