using Content.Server.Pulling;
using Content.Shared.Damage.Systems;
using Content.Shared.Pulling.Components;
using Content.Shared.Pulling.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.SimpleStation14.Species.Shadowkin.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinTeleportSystem : EntitySystem
    {
        [Dependency] private readonly ShadowkinPowerSystem _powerSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly PullingSystem _pulling = default!;

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

            SharedPullableComponent? pullable = null; // To avoid "might not be initialized when accessed" warning
            if (_entityManager.TryGetComponent<SharedPullerComponent>(args.Performer, out var puller) &&
                puller.Pulling != null &&
                _entityManager.TryGetComponent<SharedPullableComponent>(puller.Pulling, out pullable) &&
                pullable.BeingPulled)
            {
                // Temporarily stop pulling to avoid not teleporting to the target
                _pulling.TryStopPull(pullable);
            }

            // Teleport the performer to the target
            _transformSystem.SetCoordinates(args.Performer, args.Target);
            transform.AttachToGridOrMap();

            if (pullable != null && puller != null)
            {
                // Get transform of the pulled entity
                var pulledTransform = Transform(pullable.Owner);

                // Teleport the pulled entity to the target
                // TODO: Relative position to the performer
                _transformSystem.SetCoordinates(pullable.Owner, args.Target);
                pulledTransform.AttachToGridOrMap();

                // Resume pulling
                // TODO: This does nothing?
                _pulling.TryStartPull(puller, pullable);
            }


            // Play the teleport sound
            _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

            // Take power and deal stamina damage
            _powerSystem.TryAddPowerLevel(comp.Owner, -args.PowerCost);
            _staminaSystem.TakeStaminaDamage(args.Performer, args.StaminaCost);

            args.Handled = true;
        }
    }
}
