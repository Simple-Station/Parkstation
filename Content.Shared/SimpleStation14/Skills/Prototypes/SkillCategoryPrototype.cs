using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Skills.Prototypes;

[Prototype("skillCategory")]
public sealed class SkillCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    // /// <summary>
    // ///     The Sprite Specifier for the icon for this category.
    // /// </summary>
    // [DataField("icon")]
    // public SpriteSpecifier Icon = default!;

    // /// <summary>
    // ///    A whitelist to determine what Entities are allowed to use this category of skills.
    // /// </summary>
    // [DataField("whitelist")]
    // public EntityWhitelist? Whitelist = null;

    // /// <summary>
    // ///    A whitelist to determine what Entities are NOT allowed to use this category of skills.
    // /// </summary>
    // /// <remarks>
    // ///    A blacklist is just a whitelist you deny.
    // /// </remarks>
    // [DataField("blacklist")]
    // public EntityWhitelist? Blacklist = null;

    /// <summary>
    ///     Whether or not this category is viewable in the character menu.
    /// </summary>
    /// <remarks>
    ///     Note this can be overriden on a per-sub category basis.
    /// </remarks>
    [DataField("viewable")]
    public bool Viewable = true;

    public string Name => Loc.GetString($"{ID}-name");

    public string Description => Loc.GetString($"{ID}-description");
}
