using Content.Shared.SimpleStation14.StationAI.Events;

namespace Content.Server.SimpleStation14.StationAI.Systems
{
    public sealed class AICameraWarp : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AICameraWarpMessage>(HandleCameraWarpMessage);
        }

        private void HandleCameraWarpMessage(AICameraWarpMessage args)
        {
            if (!_entityManager.TryGetComponent<TransformComponent>(args.Owner, out var transform)) return;
            if (!_entityManager.TryGetComponent<TransformComponent>(args.Camera, out var cameraTransform)) return;

            if (transform.MapID != cameraTransform.MapID) return;

            _transformSystem.SetCoordinates(args.Owner, cameraTransform.Coordinates);
            transform.AttachToGridOrMap();
        }
    }
}
