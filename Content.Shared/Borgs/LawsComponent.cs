using Robust.Shared.GameStates;

namespace Content.Shared.Borgs
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class LawsComponent : Component
    {
        [DataField("laws"), AutoNetworkedField]
        public List<string> Laws = new();

        [DataField("canState"), AutoNetworkedField]
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
        [DataField("lawsID"), AutoNetworkedField]
        public string? LawsID;
        // Parkstation-Laws-End
    }
}
