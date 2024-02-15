using Content.Shared.SimpleStation14.StationAI;
using Content.Shared.SimpleStation14.StationAI.Events;
using Robust.Shared.Map;

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

            var aiTransform = Transform(args.Owner);
            if (aiTransform.GridUid == null)
                return; // Not dealing with this right now.

            var warpPosition = args.Coords;

            _transformSystem.SetCoordinates(args.Owner, warpPosition);
            _transformSystem.AttachToGridOrMap(args.Owner, aiTransform);
        }
    }
}
