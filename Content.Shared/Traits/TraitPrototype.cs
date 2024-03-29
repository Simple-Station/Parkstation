using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using static Robust.Shared.Prototypes.EntityPrototype; // don't worry about it

namespace Content.Shared.Traits
{
    /// <summary>
    ///     Describes a trait.
    /// </summary>
    [Prototype("trait")]
    public sealed class TraitPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; } = default!;

        /// <summary>
        ///     The name of this trait.
        /// </summary>
        [DataField("name")]
        public string Name { get; private set; } = "";

        /// <summary>
        ///     The description of this trait.
        /// </summary>
        [DataField("description")]
        public string? Description { get; private set; }

        /// <summary>
        ///     The category type of this trait.
        /// </summary>
        [DataField("category")]
        public string Category = "Equal";

        /// <summary>
        ///     The point cost of this trait.
        /// </summary>
        [DataField("cost")]
        public int Cost = 0;

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS NOT valid for.
        /// </summary>
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist;

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS valid for. (hence, a blacklist)
        /// </summary>
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        /// <summary>
        ///     The components that get added to the player, when they pick this trait.
        /// </summary>
        [DataField("components")]
        public ComponentRegistry Components { get; } = default!;

        /// <summary>
        ///     Gear that is given to the player, when they pick this trait.
        /// </summary>
        [DataField("traitGear", required: false, customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? TraitGear;
    }
}
