using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Roles
{
    [Prototype("startingGear")]
    public sealed class StartingGearPrototype : IPrototype
    {
        // TODO: Custom TypeSerializer for dictionary value prototype IDs
        [DataField("equipment")] private Dictionary<string, string> _equipment = new();

        /// <summary>
        /// if empty, there is no skirt override - instead the uniform provided in equipment is added.
        /// </summary>
        [DataField("innerclothingskirt", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string? _innerClothingSkirt;

        [DataField("satchel", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string? _satchel;

        [DataField("duffelbag", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string? _duffelbag;

        // Underwear

        [DataField("underpants", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string _underpants = "ClothingUnderboxer_briefs";

        [DataField("underpantsskirt", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string _underpantsskirt = "ClothingUnderpanties";

        [DataField("undershirt", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string _undershirt = "ClothingUnderundershirt";

        [DataField("undershirtskirt", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string _undershirtskirt = "ClothingUnderbra";

        [DataField("undersocks", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        private string _undersocks = "ClothingUnderSocks_norm";

        public IReadOnlyDictionary<string, string> Inhand => _inHand;
        /// <summary>
        /// hand index, item prototype
        /// </summary>
        [DataField("inhand")]
        private Dictionary<string, string> _inHand = new(0);

        [ViewVariables]
        [IdDataField]
        public string ID { get; } = string.Empty;

        public string GetGear(string slot, HumanoidCharacterProfile? profile)
        {
            if (profile != null)
            {
                if (slot == "jumpsuit" && profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(_innerClothingSkirt))
                    return _innerClothingSkirt;
                if (slot == "back" && profile.Backpack == BackpackPreference.Satchel && !string.IsNullOrEmpty(_satchel))
                    return _satchel;
                if (slot == "back" && profile.Backpack == BackpackPreference.Duffelbag && !string.IsNullOrEmpty(_duffelbag))
                    return _duffelbag;


                // Handles equipping all crew with underwear without putting it in every file.
                // Checks for skirt settings, if skirt = true, equip with panties and bra
                // if skirt = false, equip with boxers and shirt.
                // Using else caused weird issues I didn't feel like dealing with -Psp

                if (slot == "underpants" && profile.Clothing != ClothingPreference.Jumpskirt && string.IsNullOrEmpty(_underpants))
                    return "ClothingUnderboxer_briefs";
                if (slot == "underpants" && profile.Clothing == ClothingPreference.Jumpskirt && string.IsNullOrEmpty(_underpantsskirt))
                    return "ClothingUnderpanties";

                if (slot == "undershirt" && profile.Clothing != ClothingPreference.Jumpskirt && string.IsNullOrEmpty(_undershirt))
                    return "ClothingUnderundershirt";
                if (slot == "undershirt" && profile.Clothing == ClothingPreference.Jumpskirt && string.IsNullOrEmpty(_undershirtskirt))
                    return "ClothingUnderbra";

                if (slot == "socks" && string.IsNullOrEmpty(_undersocks))
                    return "ClothingUnderSocks_norm";

                // Handles custom underwear per role.

                if (slot == "underpants" && profile.Clothing != ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(_underpants))
                    return _underpants;
                if (slot == "underpants" && profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(_underpantsskirt))
                    return _underpantsskirt;

                if (slot == "undershirt" && profile.Clothing != ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(_undershirt))
                    return _undershirt;
                if (slot == "undershirt" && profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(_undershirtskirt))
                    return _undershirtskirt;

                if (slot == "socks" && !string.IsNullOrEmpty(_undersocks))
                    return _undersocks;
            }

            return _equipment.TryGetValue(slot, out var equipment) ? equipment : string.Empty;
        }
    }
}
