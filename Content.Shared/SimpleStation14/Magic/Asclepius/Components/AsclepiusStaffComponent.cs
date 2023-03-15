using System.Threading;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Magic.Asclepius.Components
{
    [RegisterComponent]
    public class AsclepiusStaffComponent : Component
    {
        /// <summary>
        ///     Token for interrupting a do-after action (e.g., injection another player). If not null, implies
        ///     component is currently "in use".
        /// </summary>
        public CancellationTokenSource? CancelToken;

        /// <summary>
        ///     Is the oath being taken on this?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Active = false;

        /// <summary>
        ///     Did the oath fail?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Failed = false;


        /// <summary>
        ///     Who owns this?
        ///     Who to give extra benefits to?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public EntityUid BoundTo = EntityUid.Invalid;

        /// <summary>
        ///     Makes the bound entity pacified.
        ///     Only takes effect when binding.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public bool PacifyBound = true;


        /// <summary>
        ///     Refuse deletion or removal of this staff, keep it in hand at all costs.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool RegenerateOnRemoval = true;


        /// <summary>
        ///     Doesn't remove the oath ever, though the staff is *potentially* removable.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool PermanentOath = false;

        /// <summary>
        ///     Spawn the healing Asclepius snake when the bound entity dies.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool SnakeOnDeath = true;
    }

    [Serializable, NetSerializable]
    public sealed class AsclepiusStaffComponentState : ComponentState
    {
        public EntityUid BoundTo { get; init; }
        public bool PacifyBound { get; init; }
        public bool RegenerateOnRemoval { get; init; }
        public bool PermanentOath { get; init; }
    }
}
