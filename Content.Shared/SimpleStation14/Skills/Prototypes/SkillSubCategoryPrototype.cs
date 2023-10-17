using Content.Shared.SimpleStation14.Skills.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;
using Content.Shared.Whitelist;

using System.Linq;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.SimpleStation14.Skills.Prototypes;

[Prototype("skillSubCategory")]
public sealed class SkillSubCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The primary category this sub category belongs to.
    /// </summary>
    [DataField("category", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<SkillCategoryPrototype>))]
    public string Category = default!;

    // /// <summary>
    // ///     The Sprite Specifier for the icon for this sub category.
    // ///     This directory will also be used to find the icons for the skills in this sub category.
    // /// </summary>
    // [DataField("icon")]
    // public SpriteSpecifier Icon = default!;

    /// <summary>
    ///     Color for this sub category.
    ///     Used to color the icons of the skills.
    /// </summary>
    [DataField("color", customTypeSerializer: typeof(ColorSerializer))]
    public Color SubCategoryColor = Color.FromHex("#A0A0A0");

    /// <summary>
    ///    A whitelist to determine what Entities are allowed to use this sub category of skills.
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist = null;

    /// <summary>
    ///    A whitelist to determine what Entities are NOT allowed to use this sub category of skills.
    /// </summary>
    /// <remarks>
    ///    A blacklist is just a whitelist you deny.
    /// </remarks>
    [DataField("blacklist")]
    public EntityWhitelist? Blacklist = null;

    /// <summary>
    ///     Whether or not this sub category is selectable by players in character creation.
    /// </summary>
    /// <remarks>
    ///     Note this can be overriden on a per-skill basis.
    /// </remarks>
    [DataField("selectable")]
    public bool Selectable = true;

    /// <summary>
    ///     Whether or not this sub category is viewable in the character menu.
    /// </summary>
    /// <remarks>
    ///     Note this can be overriden on a per-skill basis.
    /// </remarks>
    [DataField("viewable")]
    public bool Viewable = true;

    public string Name => Loc.GetString($"{ID}-name");

    public string Description => Loc.GetString($"{ID}-description");

    // /// <summary>
    // ///    A list of every skill in this sub category.
    // /// </summary>
    // /// <remarks>
    // ///     Each entry is a Dictionary that must contain a unique name for the skill.
    // ///     Optionally, it can also contain a 'description' loc key string, a 'selectableOverride' bool, and an 'icon' state string.
    // ///     If not included, the description and icon will be generated from the skill's name, and the selectable bool will be inherited from the sub category.
    // /// </remarks>
    // [DataField("skills", customTypeSerializer: typeof(SkillListSerializer))]
    // public List<Dictionary<string, string>> Skills = new();
}

// public sealed class SkillListSerializer : ITypeValidator<List<Dictionary<string, string>>, SequenceDataNode>
// {
//     private static readonly Dictionary<string, Type> ValidKeys = new()
//     {
//         {"selectableOverride", typeof(bool)},
//         {"description", typeof(string)},
//         {"icon", typeof(string)},
//         {"name", typeof(string)},
//     };

//     private static bool CheckValidKey(DataNode node)
//     {
//         if (node is not ValueDataNode value)
//             return false;

//         return ValidKeys.ContainsKey(value.Value);
//     }

//     public ValidationNode Validate(
//         ISerializationManager serializationManager,
//         SequenceDataNode node,
//         IDependencyCollection dependencies,
//         ISerializationContext? context = null)
//     {
//         var sequence = new List<ValidationNode>();

//         foreach (var mappingNode in new List<DataNode>(node.Sequence))
//         {
//             // Check if the entry in the sequence is a mapping.
//             if (mappingNode is not MappingDataNode mapping)
//             {
//                 sequence.Add(new ErrorNode(mappingNode, "SkillListSerializer prototypes must be a list of mappings/dictionaries."));
//                 continue;
//             }

//             // Check if the mapping has a 'name' key, and that it's a string.
//             if (!mapping.TryGet("name", out var nameNode) || nameNode is not ValueDataNode)
//             {
//                 sequence.Add(new ErrorNode(mapping, "SkillListSerializer prototype entries must have a 'name' key representing a string."));
//                 continue;
//             }

//             // Check that all the keys present are contained in the valid keys list.
//             if (mapping.Keys.Any(key => !CheckValidKey(key)))
//             {
//                 sequence.Add(new ErrorNode(mapping, $"SkillListSerializer prototype entries must only contain valid keys. Valid keys are: {string.Join(", ", ValidKeys.Keys)}"));
//                 continue;
//             }
//         }

//         return new ValidatedSequenceNode(sequence);
//     }

// }
