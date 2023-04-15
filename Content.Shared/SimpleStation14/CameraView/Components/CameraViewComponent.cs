using Robust.Shared.GameStates;

namespace Content.Shared.CameraView.Components
{
    /// <summary>
    ///     This isn't intended to be on master, but it is, so it's staying.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed class CameraViewComponent : Component
    {
        /// <summary>
        /// The vehicle this rider is currently riding.
        /// </summary>
        [ViewVariables] public EntityUid? Vehicle;

        public override bool SendOnlyToOwner => true;
    }
}
