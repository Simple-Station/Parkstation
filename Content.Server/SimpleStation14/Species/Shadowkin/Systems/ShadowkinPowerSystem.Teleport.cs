using Content.Shared.Damage.Systems;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinTeleportEvent>(Teleport);
        }

        private void Teleport(ShadowkinTeleportEvent args)
        {
            if (args.Handled)
                return;

            if (!_entityManager.TryGetComponent<ShadowkinComponent>(args.Performer, out var comp))
                return;

            var transform = Transform(args.Performer);
            if (transform.MapID != args.Target.GetMapId(EntityManager))
                return;

            _transformSystem.SetCoordinates(args.Performer, args.Target);
            transform.AttachToGridOrMap();

            _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

            _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCost);
            _staminaSystem.TakeStaminaDamage(args.Performer, args.StaminaCost);

            args.Handled = true;
        }
    }
}
