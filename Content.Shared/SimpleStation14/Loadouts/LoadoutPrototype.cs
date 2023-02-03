using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Loadouts
{
    /// <summary>
    ///     Describes a loadout.
    /// </summary>
    [Prototype("loadout")]
    public sealed class LoadoutPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; } = default!;

        /// <summary>
        ///     The name of this loadout.
        /// </summary>
        [DataField("name")]
        public string Name { get; private set; } = "";

        /// <summary>
        ///     The description of this loadout.
        /// </summary>
        [DataField("description")]
        public string? Description { get; private set; }

        /// <summary>
        ///     Which tab category to put this under.
        /// </summary>
        [DataField("category")]
        public string Category { get; private set; } = "Uncategorized";

        /// <summary>
        ///     Which tab category to put this under.
        /// </summary>
        [DataField("categoryNum")]
        public int CategoryNum { get; private set; } = 0; // Awful solution 👍

        /// <summary>
        ///     The point cost of this loadout.
        /// </summary>
        [DataField("cost")]
        public int Cost = 0;

        /// <summary>
        ///     Don't apply this loadout to entities this whitelist IS NOT valid for.
        /// </summary>
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist;

        /// <summary>
        ///     Don't apply this loadout to entities this whitelist IS valid for. (hence, a blacklist)
        /// </summary>
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        [DataField("item")]
        public string? Item;
    }
}
