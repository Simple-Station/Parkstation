using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.SimpleStation14.Skills.Components;
using Content.Shared.SimpleStation14.Skills.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.SimpleStation14.Skills.Systems;

public sealed class SharedSkillsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private Dictionary<SkillCategoryPrototype, Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>> _categorizedSkills = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SkillsComponent, MapInitEvent>(OnSkillsInit);

        GenerateCategorizedSkills();
        _prototype.PrototypesReloaded += OnPrototypesReloaded;
    }

    # region Public Functions
    /// <summary> Forces the level of a skill for a given entity. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <param name="level"> The value to set the skill level to. </param>
    /// <remarks> WARNING: This bypasses several checks, such as species restrictions, max skill level, etc.. Use sparingly. </remarks>
    public void ForceSkillLevel(EntityUid uid, string skillId, int level, SkillsComponent? skillsComp = null)
    {
        SetSkillInternal(uid, skillId, level, skillsComp, true);
    }

    /// <summary> Tries to modify the level of a skill for a given entity. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <param name="level"> The amount to modify the skill level by. </param>
    /// <returns> True if the skill level was successfully modified, false otherwise. </returns>
    public bool TryModifySkillLevel(EntityUid uid, string skillId, int level, SkillsComponent? skillsComp = null)
    {
        return SetSkillInternal(uid, skillId, GetSkillLevel(uid, skillId, skillsComp) + level, skillsComp);
    }

    /// <summary> Tries to set the level of a skill for a given entity. </summary>
    /// <param name="level"> The value to set the skill level to. </param>
    /// <inheritdoc cref="TryModifySkillLevel(EntityUid,string,int,Content.Shared.SimpleStation14.Skills.Components.SkillsComponent?)"/>
    public bool TrySetSkillLevel(EntityUid uid, string skillId, int level, SkillsComponent? skillsComp = null)
    {
        return SetSkillInternal(uid, skillId, level, skillsComp);
    }

    /// <summary> Tries to get the level of a specific skill for an entity. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <param name="level"> The level of the skill, or 0 if none is found. </param>
    /// <returns> True if the entity is capable of having skills, otherwise false. </returns>
    public bool TryGetSkillLevel(EntityUid uid, string skillId, out int level, SkillsComponent? skillsComp = null, bool raw = false)
    {
        return GetSkillInternal(uid, skillId, out level, skillsComp, raw);
    }

    /// <summary> Returns the level of an entity's skill, defaulting to 0. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> The level of the entity's skill. </returns>
    public int GetSkillLevel(EntityUid uid, string skillId, SkillsComponent? skillsComp = null, bool raw = false)
    {
        if (!TryGetSkillLevel(uid, skillId, out var level, skillsComp, raw))
            return 0;
        return level;
    }

    /// <summary> Returns the overall level of a set of skills, altered by their weights. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <param name="skillIds"> A list of skill IDs to get the average skill level of. </param>
    /// <returns> The average of all the skills as a float, or zero if none could be found. </returns>
    public float GetSkillAverages(EntityUid uid, List<string> skillIds, SkillsComponent? skillsComp = null, bool raw = false)
    {
        return GetSkillAverages(uid, skillIds.ConvertAll(_prototype.Index<SkillPrototype>), skillsComp, raw);
    }

    /// <inheritdoc cref="GetSkillAverages(EntityUid,List{string},Content.Shared.SimpleStation14.Skills.Components.SkillsComponent?)"/>
    /// <param name="skills"> A list of skill prototypes to get the average skill level of. </param>
    public float GetSkillAverages(EntityUid uid, List<SkillPrototype> skills, SkillsComponent? skillsComp = null, bool raw = false)
    {
        return GetSkillAverages(uid, skillWeights: skills.ToDictionary(skill => skill, skill => skill.Weight), skillsComp, raw);
    }

    /// <inheritdoc cref="GetSkillAverages(EntityUid,List{string},Content.Shared.SimpleStation14.Skills.Components.SkillsComponent?)"/>
    /// <param name="skillIdWeights"> A dictionary of skill IDs and their weights to get the average skill level of. </param>
    /// <remarks> Uses custom weights for each skill, rather than their default. </remarks>
    public float GetSkillAverages(EntityUid uid, Dictionary<string, float> skillIdWeights, SkillsComponent? skillsComp = null, bool raw = false)
    {
        return GetSkillAverages(uid, skillWeights: skillIdWeights.ToDictionary(pair => _prototype.Index<SkillPrototype>(pair.Key), pair => pair.Value), skillsComp, raw);
    }

    /// <inheritdoc cref="GetSkillAverages(EntityUid,Dictionary{string,float},Content.Shared.SimpleStation14.Skills.Components.SkillsComponent?)"/>
    /// <param name="skillWeights"> A dictionary of skill prototypes and their weights to get the average skill level of. </param>
    public float GetSkillAverages(EntityUid uid, Dictionary<SkillPrototype, float> skillWeights, SkillsComponent? skillsComp = null, bool raw = false)
    {
        var totalWeight = 0f;
        var totalLevel = 0f;

        foreach (var (skill, weight) in skillWeights)
        {
            if (!GetSkillInternal(uid, skill.ID, out var level, skillsComp, raw))
                continue;

            totalLevel += level * weight;
            totalWeight += weight;
        }

        return totalLevel / totalWeight;
    }

    /// <summary> Returns the overall level of a sub category.
    /// This is an average of all the skills in the sub category, considering their individual weights. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    public float GetSubCategoryLevel(EntityUid uid, string subCategoryId, SkillsComponent? skillsComp = null)
    {
        if (!TryGetSkillSubCategory(subCategoryId, out var subCategory))
            return 0;

        return GetSubCategoryLevel(uid, subCategory, skillsComp);
    }

    /// <inheritdoc cref="GetSubCategoryLevel(EntityUid,string,Content.Shared.SimpleStation14.Skills.Components.SkillsComponent?)"/>
    public float GetSubCategoryLevel(EntityUid uid, SkillSubCategoryPrototype subCategory, SkillsComponent? skillsComp = null)
    {
        if (!TryGetSkillCategory(subCategory.Category, out var category))
            return 0;

        return GetSkillAverages(uid, _categorizedSkills[category][subCategory], skillsComp);
    }

    /// <summary> Used to determine whether or not an Entity is 'trained' in a particular skill. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> True if the entity has at least one level in the skill. </returns>
    public bool IsSkillTrained(EntityUid uid, string skillId, SkillsComponent? skillsComp = null)
    {
        return GetSkillLevel(uid, skillId, skillsComp) > 0;
    }

    /// <summary>
    ///     Returns a Dictionary of all the skills an Entity has levels in. </summary>
    /// <remarks> Note that this will only return skills that the Entity has greater than 0 levels in i.e. are trained in. </remarks>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> A Dictionary of skills and levels. </returns>
    /// <returns> True if the entity is capable of having skills, false otherwise. </returns>
    public bool TryGetAllSkillLevels(EntityUid uid, out Dictionary<string, int> skillLevels, SkillsComponent? skillsComp = null)
    {
        skillLevels = new Dictionary<string, int>();

        if (!Resolve(uid, ref skillsComp))
            return false;

        foreach (var skillId in skillsComp.Skills.Keys)
        {
            if (!GetSkillInternal(uid, skillId, out var level, skillsComp))
                continue;

            skillLevels.Add(skillId, level);
        }

        return true;
    }

    /// <summary> Gets all skills in the game organized by category, and then sub category. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    public Dictionary<SkillCategoryPrototype, Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>> GetAllSkillsCategorized()
    {
        return _categorizedSkills;
    }

    /// <summary> Tries to get all skills in a given category. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> True if the category exists, false otherwise. </returns>
    public bool TryGetSkillsInCategory(string categoryId, [NotNullWhen(true)] out Dictionary<SkillSubCategoryPrototype, List<SkillPrototype>>? skillsBySubCategory)
    {
        skillsBySubCategory = null;

        if (!SkillIndex(categoryId, out SkillCategoryPrototype? category))
            return false;

        if (!_categorizedSkills.TryGetValue(category, out skillsBySubCategory))
            return false;

        return true;
    }

    /// <summary> Tries to get all skills in a given sub category. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <param name="skills"> A list of skills in the sub category. </param>
    /// <returns> True if the sub category exists, false otherwise. </returns>
    public bool TryGetSkillsInSubCategory(string subCategoryId, [NotNullWhen(true)] out List<SkillPrototype>? skills)
    {
        skills = null;

        if (!SkillIndex(subCategoryId, out SkillSubCategoryPrototype? subCategory))
            return false;

        if (!TryGetSkillsInCategory(subCategory.Category, out var skillsBySubCategory))
            return false;

        if (!skillsBySubCategory.TryGetValue(subCategory, out skills))
            return false;

        return true;
    }

    /// <summary> Tries to get a skill by its ID. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> True if the skill was found, false otherwise. </returns>
    public bool TryGetSkill(string skillId, [NotNullWhen(true)] out SkillPrototype? skill)
    {
        return SkillIndex(skillId, out skill);
    }

    /// <summary> Tries to get a skill subcategory by its ID. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> True if the skill subcategory was found, false otherwise. </returns>
    public bool TryGetSkillSubCategory(string subCategoryId, [NotNullWhen(true)] out SkillSubCategoryPrototype? subCategory)
    {
        return SkillIndex(subCategoryId, out subCategory);
    }

    /// <summary> Tries to get a skill category by its ID. </summary>
    /// <inheritdoc cref="DefaultXmlDocs"/>
    /// <returns> True if the skill category was found, false otherwise. </returns>
    public bool TryGetSkillCategory(string categoryId, [NotNullWhen(true)] out SkillCategoryPrototype? category)
    {
        return SkillIndex(categoryId, out category);
    }

    #endregion
    #region Private Functions
    private void OnSkillsInit(EntityUid uid, SkillsComponent component, MapInitEvent args)
    {
        foreach (var startSkill in component.StartingSkills)
            TryModifySkillLevel(uid, startSkill.Key, startSkill.Value, component);
    }

    private bool GetSkillInternal(EntityUid uid, string skillId, out int level, SkillsComponent? skillsComp = null, bool raw = false)
    {
        level = 0;

        if (!Resolve(uid, ref skillsComp))
            return false;

        if (!SkillIndex<SkillPrototype>(skillId, out _))
            return false;

        skillsComp.Skills.TryGetValue(skillId, out level);

        if (!raw)
        {
            var ev = new GetSkillEvent(skillId, level);
            RaiseLocalEvent(uid, ref ev);

            level = ev.Level;
        }

        return true;
    }

    private bool SetSkillInternal(EntityUid uid, string skillId, int level, SkillsComponent? skillsComp = null, bool force = false)
    {
        if (!SkillIndex<SkillPrototype>(skillId, out var skill))
            return false; // Return if the skill isn't real.

        return SetSkillInternal(uid, skill, level, skillsComp, force);
    }

    private bool SetSkillInternal(EntityUid uid, SkillPrototype skill, int level, SkillsComponent? skillsComp = null, bool force = false)
    {
        if (!Resolve(uid, ref skillsComp))
            return false;

        // If the skill is on the blacklist, or not on the whitelist, return.
        if (!force &&
            (skill.Whitelist != null && !skill.Whitelist.IsValid(uid) || skill.Blacklist != null && skill.Blacklist.IsValid(uid)))
            return false;

        // Cap the skill to its own max value.
        if (!force)
            level = level > skill.MaxValue ? skill.MaxValue : level;

        if (!force)
        {
            var cancelEvent = new SkillModifyAttemptEvent(skill.ID, level, GetSkillLevel(uid, skill.ID, skillsComp, true));
            RaiseLocalEvent(uid, ref cancelEvent);
            if (cancelEvent.Cancelled)
                return false;
        }

        var ev = new SkillModifiedEvent(skill.ID, level, GetSkillLevel(uid, skill.ID, skillsComp, true));
        RaiseLocalEvent(uid, ref ev);

        // If the level is 0 or less, simply remove the skill from their component.
        if (level <= 0)
        {
            skillsComp.Skills.Remove(skill.ID);
            return true;
        }

        // Finally, set or add the skill to the component.
        if (skillsComp.Skills.ContainsKey(skill.ID))
            skillsComp.Skills[skill.ID] = level;
        else
            skillsComp.Skills.Add(skill.ID, level);

        Dirty(skillsComp);
        return true;
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

    private bool SkillIndex<T>(string prototypeId, [NotNullWhen(true)] out T? prototype) where T : class, IPrototype
    {
        if (!_prototype.TryIndex(prototypeId, out prototype))
        {
            Log.Warning($"Unknown {typeof(T).Name} prototype: {prototypeId}");
            return false;
        }
        return true;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        GenerateCategorizedSkills();
    }

    // This is pretty goofy, I know, but how many times am I repeating these parameters?
    /// <param name="uid"> The entity UID. </param>
    /// <param name="skillsComp"> The SkillsComponent to check on, if avaliable. </param>
    /// <param name="skill"> The skill prototype. </param>
    /// <param name="skillId"> The ID of the skill. </param>
    /// <param name="category"> The skill category prototype. </param>
    /// <param name="categoryId"> The ID of the skill category. </param>
    /// <param name="subCategory"> The skill sub category prototype. </param>
    /// <param name="subCategoryId"> The ID of the skill sub category. </param>
    private static void DefaultXmlDocs()
    {
    }
    #endregion
}

#region Events
[ByRefEvent]
/// <summary> Cancellable Event called before a skill level is modified. Can be used to modify the new value. </summary>
/// <param name="SkillId"> The skill prototype ID. </param>
/// <param name="NewLevel"> The new level of the skill. </param>
/// <param name="OldLevel"> The old level of the skill. </param>
public record struct SkillModifyAttemptEvent(string SkillId, int NewLevel, int OldLevel)
{
    public int NewLevel = NewLevel;
    public bool Cancelled = false;

    public readonly string SkillId = SkillId;
    public readonly int OldLevel = OldLevel;
    public readonly bool Increased => NewLevel > OldLevel;
}

[ByRefEvent]
/// <summary> Event called when a skill level is modified. </summary>
/// <inheritdoc cref="SkillModifyAttemptEvent"/>
public readonly record struct SkillModifiedEvent(string SkillId, int NewLevel, int OldLevel)
{
    // /// <summary> The ID of the skill being modified. </summary>
    // public readonly string SkillId = SkillId;

    // /// <summary> The new level of the skill. </summary>
    // public readonly int NewLevel = NewLevel;

    // /// <summary> The old level of the skill. </summary>
    // public readonly int OldLevel = OldLevel;

    /// <summary> Whether the skill level was increased (true) or decreased (false). </summary>
    public readonly bool Increased => NewLevel > OldLevel;
}

/// <summary> Event called when attempting to get a skill level.
/// Can be used to modify the level returned. </summary>
/// <param name="SkillId"> The skill prototype ID. </param>
[ByRefEvent]
public record struct GetSkillEvent(string SkillId, int Level)
{
    /// <summary> The ID of the skill being checked. </summary>
    public readonly string SkillId = SkillId;

    /// <summary> The level of the skill. </summary>
    public int Level = Level;
}
#endregion
#region Various Data

// [DataDefinition, Serializable, NetSerializable]
// public struct SkillUseData
// {
//     /// <summary> The skill prototype IDs to use. </summary>
//     /// <remarks> Overrides <see cref="SkillSubCategory"/> if not null. </remarks>
//     [DataField("skills", customTypeSerializer: typeof(PrototypeIdListSerializer<SkillPrototype>))]
//     public List<SkillPrototype>? Skills;

//     /// <summary> The skill sub category prototype IDs to use. </summary>
//     /// <remarks> Ignored if <see cref="Skills"/> is not null. </remarks>
//     [DataField("skillSubCategory", customTypeSerializer: typeof(PrototypeIdSerializer<SkillSubCategoryPrototype>))]
//     public string? SkillSubCategory;

//     /// <summary> Are you required to be trained in the skill to use it? </summary>
//     /// <remarks> If used with a list of skills, the individual skill will be ignored if not trained. </remarks>
//     [DataField("requireTrained")]
//     public bool RequireTrained = false;
// }


[NetSerializable, Serializable]
public enum SkillsUiKey : byte
{
    Key,
}
#endregion
