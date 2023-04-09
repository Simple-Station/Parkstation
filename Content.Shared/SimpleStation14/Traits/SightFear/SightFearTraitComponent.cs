using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Traits.SightFear
{
    [RegisterComponent]
    public sealed class SightFearTraitComponent : Component
    {
        /// <summary>
        ///     The list of prototypes that the entity can be afraid of.
        /// </summary>
        [DataField("prototypes")]
        public List<string> PossiblePrototypes { get; set; } = new List<string>();

        /// <summary>
        ///     What prototypes they are currently afraid of.
        ///     Defaults can be specified by the DataField.
        /// </summary>
        [DataField("afraidOfPrototypes")]
        public List<string> AfraidOfPrototypes { get; set; } = new List<string>();

        /// <summary>
        ///     The list of tags that the entity can be afraid of.
        /// </summary>
        [DataField("tags")]
        public List<string> PossibleTags { get; set; } = new List<string>();

        /// <summary>
        ///     What tags they are currently afraid of.
        ///     Defaults can be specified by the DataField.
        /// </summary>
        [DataField("afraidOfTags")]
        public List<string> AfraidOfTags { get; set; } = new List<string>();


        /// <summary>
        ///     The chance the entity will have multiple fears as a decimal percent (0-1).
        ///     Whenever a fear is added, it rolls the chance to add another fear until it fails or it's afraid of everything.
        /// </summary>
        [DataField("multipleFearChance")]
        public float MultipleFearChance { get; set; } = 0f;

        /// <summary>
        ///     How many fears can be added by the multiple fear chance.
        /// </summary>
        [DataField("maxFears")]
        public int MaxFears { get; set; } = 1;
    }

    [NetSerializable, Serializable]
    public sealed class SightFearTraitComponentState : ComponentState
    {
        public List<string> AfraidOfPrototypes { get; }
        public List<string> AfraidOfTags { get; }

        public SightFearTraitComponentState(List<string> afraidOfPrototypes, List<string> afraidOfTags)
        {
            AfraidOfPrototypes = afraidOfPrototypes;
            AfraidOfTags = afraidOfTags;
        }
    }
}
