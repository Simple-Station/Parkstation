using Robust.Client.GameObjects;
using Content.Shared.SimpleStation14.StationAI.Events;
using Robust.Shared.Physics;

namespace Content.Client.SimpleStation14.StationAI.UI
{
    /// <summary>
    ///     Initializes a <see cref="AICameraList"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class AICameraListBoundUserInterface : BoundUserInterface
    {
        // [Dependency] private readonly TransformSystem _transform = default!;

        private AICameraList _window = new AICameraList();

        public AICameraListBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _window.TryUpdateCameraList += () => SendMessage(new AICameraListMessage(Owner));
            _window.PositionSelected += (pos) => SendMessage(new AICameraWarpMessage(Owner, pos));
        }

        protected override void Open()
        {
            base.Open();

            if (State != null) UpdateState(State);
            _window.SetGrid(Owner);

            _window.OpenCentered();
        }

        /// <summary>
        ///     Update the UI state based on server-sent info
        /// </summary>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (_window == null || state is not AIBoundUserInterfaceState cast)
                return;

            _window.UpdateCameraList(cast.Cameras);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window.Dispose();
        }
    }
}
