using Content.Shared.FixedPoint;

namespace Content.Shared.SimpleStation14.Traits.SightFear
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class SightFearTraitComponent : Component
    {
        /// <summary>
        ///     The ID of the fear this entity has
        ///     Matches this to fearable entities with the same ID and adds to <see cref="Fear"/>
        ///     If empty, a random fear will be picked from the weighted random prototype "RandomFears"
        /// </summary>
        [DataField("afraidOf")]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public string AfraidOf = string.Empty;

        /// <summary>
        ///     How much fear this entity has.
        ///     Goes up to <see cref="MaxFear"/> before the effects of fear start to kick in
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double Fear = 0;

        /// <summary>
        ///     How much fear this entity can have before the effects of fear start to kick in
        /// </summary>
        [DataField("maxFear")]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public double MaxFear = 20;

        /// <summary>
        ///     If this entity is currently afraid
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Afraid = false;

        /// <summary>
        ///     How fast the entity's fear goes down
        /// </summary>
        [DataField("calmRate")]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public float CalmRate = 1.5f;

        public float Accumulator = 0.084f;
    }
}
