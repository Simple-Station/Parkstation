using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.SimpleStation14.Skills.Components;
using Content.Shared.SimpleStation14.Skills.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Skills.Systems;

public sealed class SharedSkillsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private Dictionary<SkillCategoryPrototype, Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>> _categorizedSkills = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SkillsComponent, ComponentInit>(OnSkillsInit);

        GenerateCategorizedSkills();
        _prototype.PrototypesReloaded += OnPrototypesReloaded;
    }

    public bool TryModifySkillLevel(string skill, int level)
    {
        if (!_prototype.HasIndex<SkillPrototype>(skill))
            return false;


    }

    public bool TrySetSkillLevel(string skill, int level)
    {

    }

    /// <summary>
    ///    Returns the level of an entity's skill.
    /// </summary>
    /// <param name="uid">Entity to check for skills on.</param>
    /// <param name="skillId">The Prototype ID of the skill to check for.</param>
    /// <param name="skillsComp">The SkillsComponent of the entity.</param>
    /// <returns>The level of the entitie's skill.</returns>
    public int GetSkillLevel(EntityUid uid, string skillId, SkillsComponent? skillsComp = null)
    {
        if (!Resolve(uid, ref skillsComp))
            return 0;

        if (skillsComp.Skills.TryGetValue(skillId, out var level))
            return level;

        return 0;
    }

    /// <summary>
    ///     Returns the overall level of a sub category.
    ///     This is an average of all the skills in the sub category, considering their individual weights.
    /// </summary>
    public int GetSubCategoryLevel(EntityUid uid, string subCategoryId, SkillsComponent? skillsComp = null)
    {
        if (!Resolve(uid, ref skillsComp))
            return 0;

        if (!TryGetSkillSubCategory(subCategoryId, out var subCategory))
            return 0;

        if (!TryGetSkillCategory(subCategory.Category, out var category))
            return 0;

        var skills = _categorizedSkills[category][subCategory];

        var totalWeight = skills.Sum(skill => skill.Weight);
        var totalLevel = skills.Sum(skill => GetSkillLevel(uid, skill.ID, skillsComp) * skill.Weight);

        return totalLevel / totalWeight;
    }

    /// <summary>
    ///     Used to determine whether or not an Entity is 'trained' in a particular skill.
    /// </summary>
    /// <param name="uid">Entity to check for skills on.</param>
    /// <param name="skillId">The Prototype ID of the skill to check for.</param>
    /// <param name="skillsComp">The SkillsComponent of the entity.</param>
    /// <returns>True if the entity has at least one level in the skill.</returns>
    public bool IsTrained(EntityUid uid, string skillId, SkillsComponent? skillsComp = null)
    {
        if (!Resolve(uid, ref skillsComp))
            return false;

        return GetSkillLevel(uid, skillId, skillsComp) > 0;
    }

    /// <summary>
    ///     Returns a Dictionary of all the skills an Entity has levels in.
    /// </summary>
    /// <remarks>
    ///     Note that this will only be Components that happen to be logged on the Entity. They may or may not be trained, and it almost certainly won't be all of the skills in the game.
    /// </remarks>
    /// <param name="uid">Entity to check for skills on.</param>
    /// <param name="skillsComp">The SkillsComponent of the entity.</param>
    /// <returns>A Dictionary of skills and levels.</returns>
    public Dictionary<string, int> GetAllSkillLevels(EntityUid uid, SkillsComponent? skillsComp = null)
    {
        if (!Resolve(uid, ref skillsComp))
            return new Dictionary<string, int>();

        return new(skillsComp.Skills);
    }

    /// <summary>
    ///     Gets all skills in the game organized by category, and then sub category.
    /// </summary>
    public Dictionary<SkillCategoryPrototype, Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>> GetAllSkillsCategorized()
    {
        return _categorizedSkills;
    }

    /// <summary>
    ///     Tries to get all skills in a given category.
    /// </summary>
    /// <param name="categoryId">The ID of the category to get from.</param>
    /// <param name="skillsBySubCategory">A dictionary of sub categories, each containing a list of their skills.</param>
    /// <returns>True if the category exists, false otherwise.</returns>
    public bool TryGetSkillsInCategory(string categoryId, [NotNullWhen(true)] out Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>? skillsBySubCategory)
    {
        skillsBySubCategory = null;

        if (!_prototype.TryIndex(categoryId, out SkillCategoryPrototype? category))
            return false;

        if (!_categorizedSkills.TryGetValue(category, out skillsBySubCategory))
            return false;

        return true;
    }

    /// <summary>
    ///    Tries to get all skills in a given sub category.
    /// </summary>
    /// <param name="subCategoryId">The ID of the sub category to get from.</param>
    /// <param name="skills">A list of skills in the sub category.</param>
    /// <returns>True if the sub category exists, false otherwise.</returns>
    public bool TryGetSkillsInSubCategory(string subCategoryId, [NotNullWhen(true)] out List<SkillPrototype>? skills)
    {
        skills = null;

        if (!_prototype.TryIndex(subCategoryId, out SkillSubCategoryPrototype? subCategory))
            return false;

        if (!TryGetSkillsInCategory(subCategory.Category, out var skillsBySubCategory))
            return false;

        if (!skillsBySubCategory.TryGetValue(subCategory, out skills))
            return false;

        return true;
    }

    /// <summary>
    ///     Tries to get a skill prototype by its ID.
    /// </summary>
    /// <param name="skillId">The ID of the skill to get.</param>
    /// <param name="skill">The skill prototype, if found.</param>
    /// <returns>True if the skill was found, false otherwise.</returns>
    public bool TryGetSkill(string skillId, [NotNullWhen(true)] out SkillPrototype? skill)
    {
        return _prototype.TryIndex(skillId, out skill);
    }

    /// <summary>
    ///     Tries to get a skill subcategory by its ID.
    /// </summary>
    /// <param name="subCategoryId">The ID of the skill subcategory to get.</param>
    /// <param name="subCategory">The skill subcategory, if found.</param>
    /// <returns>True if the skill subcategory was found, false otherwise.</returns>
    public bool TryGetSkillSubCategory(string subCategoryId, [NotNullWhen(true)] out SkillSubCategoryPrototype? subCategory)
    {
        return _prototype.TryIndex(subCategoryId, out subCategory);
    }

    /// <summary>
    ///     Tries to get a skill category by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the skill category to get.</param>
    /// <param name="category">The skill category, if found.</param>
    /// <returns>True if the skill category was found, false otherwise.</returns>
    public bool TryGetSkillCategory(string categoryId, [NotNullWhen(true)] out SkillCategoryPrototype? category)
    {
        return _prototype.TryIndex(categoryId, out category);
    }

    private void OnSkillsInit(EntityUid uid, SkillsComponent component, ComponentInit args)
    {
        component.Skills.Add
    }

    private void SetSkill(EntityUid uid, string skillId, int level, SkillsComponent? skillsComp = null)
    {
        if (!Resolve(uid, ref skillsComp))
            return;

        if (level <= 0)
        {
            if (skillsComp.Skills.ContainsKey(skillId))
                skillsComp.Skills.Remove(skillId);
            return;
        }

        if (skillsComp.Skills.ContainsKey(skillId))
            skillsComp.Skills[skillId] = level;
        else
            skillsComp.Skills.Add(skillId, level);

        Dirty(skillsComp);
    }

    private void GenerateCategorizedSkills()
    {
        var skills = _prototype.EnumeratePrototypes<SkillPrototype>();
        var subsById = _prototype.EnumeratePrototypes<SkillSubCategoryPrototype>().ToDictionary(sub => sub.ID);
        var categoriesById = _prototype.EnumeratePrototypes<SkillCategoryPrototype>().ToDictionary(category => category.ID);

        var skillsCategorized = new Dictionary<SkillCategoryPrototype, Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>>();

        foreach (var skill in skills)
        {
            var sub = subsById[skill.SubCategory];
            var category = categoriesById[sub.Category];

            if (!skillsCategorized.ContainsKey(category))
                skillsCategorized.Add(category, new Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>());

            if (!skillsCategorized[category].ContainsKey(sub))
                skillsCategorized[category].Add(sub, new List<SkillPrototype>());

            skillsCategorized[category][sub].Add(skill);
        }

        _categorizedSkills = skillsCategorized;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        GenerateCategorizedSkills();
    }
}

[NetSerializable, Serializable]
public enum SkillsUiKey : byte
{
    Key,
}
