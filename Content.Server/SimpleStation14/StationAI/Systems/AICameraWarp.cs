using Content.Shared.SimpleStation14.StationAI;
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
            // You need to be an AI to do this.
            if (!_entityManager.TryGetComponent<AIEyeComponent>(args.Owner, out var _))
                return;

            var transform = Transform(args.Owner);
            var cameraTransform = Transform(args.Camera);

            if (transform.MapID != cameraTransform.MapID)
                return;

            _transformSystem.SetCoordinates(args.Owner, cameraTransform.Coordinates);
            transform.AttachToGridOrMap();
        }
    }
}
