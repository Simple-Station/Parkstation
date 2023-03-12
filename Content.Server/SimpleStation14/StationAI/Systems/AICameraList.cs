using Content.Server.SurveillanceCamera;
using Content.Shared.SimpleStation14.StationAI.Events;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.StationAI.Systems
{
    public sealed class AICameraList : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AICameraListMessage>(HandleCameraListMessage);
        }

        private void HandleCameraListMessage(AICameraListMessage args)
        {
            var cameras = EntityManager.EntityQuery<SurveillanceCameraComponent>();
            var cameraList = new List<EntityUid>();

            foreach (var camera in cameras)
            {
                cameraList.Add(camera.Owner);
            }

            var ui = _userInterfaceSystem.GetUi(args.Owner, args.UiKey);
            var state = new AIBoundUserInterfaceState(cameraList);
            _userInterfaceSystem.SetUiState(ui, state);
        }
    }
}
