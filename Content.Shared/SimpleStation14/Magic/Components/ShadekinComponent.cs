using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Magic.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed class ShadekinComponent : Component
    {
        /// <summary>
        ///     Used client side.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Darken = true;

        /// <summary>
        ///     Used server side.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float DarkenRange = 5f;

        /// <summary>
        ///     Used server side.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public List<EntityUid> DarkenedLights = new();

        /// <summary>
        ///     Used client side.
        /// </summary>
        /// <remarks>
        ///     You should probably not edit this.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorTime = 1f;

        /// <summary>
        ///     Used client side.
        /// </summary>
        /// <remarks>
        ///     You should probably not edit this.
        /// </remarks>
        [ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;
    }
}
