using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Borgs
{
    [RegisterComponent, NetworkedComponent]
    public sealed class LawsComponent : Component
    {
        [DataField("laws")]
        public List<string> Laws = new List<string>();

        [DataField("canState")]
        public bool CanState = true;

        /// <summary>
        ///     Antispam.
        /// </summary>
        public TimeSpan? StateTime = null;

        [DataField("stateCD")]
        public TimeSpan StateCD = TimeSpan.FromSeconds(30);

        // Parkstation-Laws-Start

        /// <summary>
        ///     The ID of either a Laws prototype, or a WeightedRandom of Laws prototypes.
        /// </summary>
        [DataField("lawsID")]
        public string? LawsID;

        // Parkstation-Laws-End
    }

    [Serializable, NetSerializable]
    public sealed class LawsComponentState : ComponentState
    {
        public readonly List<string> Laws;

        public LawsComponentState(List<string> laws)
        {
            Laws = laws;
        }
    }
}
