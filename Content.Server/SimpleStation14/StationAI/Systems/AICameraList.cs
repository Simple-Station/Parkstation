using System.Linq;
using Content.Server.SurveillanceCamera;
using Content.Shared.SimpleStation14.StationAI;
using Content.Shared.SimpleStation14.StationAI.Events;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.StationAI.Systems
{
    public sealed class AICameraList : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly TransformSystem _transform = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AICameraListMessage>(HandleCameraListMessage);
        }

        private void HandleCameraListMessage(AICameraListMessage args)
        {
            // You need to be an AI to use this.
            if (!EntityManager.TryGetComponent<AIEyeComponent>(args.Owner, out var _))
                return;

            if (!_transform.TryGetMapOrGridCoordinates(args.Owner, out var aiCoords))
                return;

            var cameraList = new List<AIBoundUserInterfaceState.CameraData>();

            var query = EntityQueryEnumerator<SurveillanceCameraComponent>();
            while (query.MoveNext(out var camera, out var cameraComp)) // Get all cameras.
                if (_transform.TryGetMapOrGridCoordinates(camera, out var coords) && coords.Value.EntityId == aiCoords.Value.EntityId) // Check if they're on the same grid as the AI.
                    cameraList.Add(new(cameraComp.CameraId, coords.Value, cameraComp.Active)); // Add all the relevant data.

            var ui = _userInterfaceSystem.GetUi(args.Owner, args.UiKey);
            var state = new AIBoundUserInterfaceState(cameraList);
            _userInterfaceSystem.TrySetUiState(args.Owner, ui.UiKey, state);
        }
    }
}
