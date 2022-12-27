using Robust.Shared.GameStates;

namespace Content.Shared.CameraView.Components
{
    /// <summary>
    /// Added to people when they are riding in a vehicle
    /// used mostly to keep track of them for entityquery.
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
