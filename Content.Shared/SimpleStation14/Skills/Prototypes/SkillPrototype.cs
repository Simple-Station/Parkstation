using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Shared.SimpleStation14.Skills.Prototypes;

[Prototype("skill")]
public sealed class SkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The sub category this skill belongs to, underneath that sub category's primary category.
    /// </summary>
    /// <remarks>
    ///     This might be something like "Melee," "Ranged," "Medical," etc..
    /// </remarks>
    [DataField("subCategory", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<SkillSubCategoryPrototype>))]
    public string SubCategory = default!;

    /// <summary>
    ///     If true, this skill's icon won't be colored by the sub category's color.
    /// </summary>
    [DataField("overrideColor")]
    public bool OverrideColor = false;

    /// <summary>
    ///     If not null, this will override the subcategory's selectable value for this skill.
    /// </summary>
    [DataField("selectableOverride")]
    public bool? SelectableOverride = null;

    /// <summary>
    ///     The wheight this skill holds in the overall sub category's level.
    ///     Default is 1, skills higher will make more of an impact in the overall level.
    ///     Skills lower will make less of an impact.
    /// </summary>
    [DataField("weight")]
    public int Weight = 1;

    /// <summary>
    ///    Optional override for the Sprite Specifier for the icon for this skill.
    /// </summary>
    /// <remarks>
    ///    If null, the state will be generated from the skill's name, and the RSI will be inherited from the sub category.
    /// </remarks>
    [DataField("icon", customTypeSerializer: typeof(SpriteSpecifierSerializer))]
    public SpriteSpecifier? Icon = null;

    /// <summary>
    ///    A whitelist to determine what Entities are allowed to use this skill, in addition to the sub categories list.
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist = null;

    /// <summary>
    ///    A whitelist to determine what Entities are NOT allowed to use this skill, in addition to the sub categories list.
    /// </summary>
    /// <remarks>
    ///    A blacklist is just a whitelist you deny.
    /// </remarks>
    [DataField("blacklist")]
    public EntityWhitelist? Blacklist = null;

    /// <summary>
    ///     Whether or not this skill is viewable in the character menu.
    /// </summary>
    [DataField("viewable")]
    public bool? Viewable = null;

    public string Name => Loc.GetString($"{ID}-name");

    public string Description => Loc.GetString($"{ID}-description");
}
