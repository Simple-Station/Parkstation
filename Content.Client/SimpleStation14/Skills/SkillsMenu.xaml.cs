using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Content.Shared.SimpleStation14.Skills.Components;
using Content.Shared.SimpleStation14.Skills.Systems;

namespace Content.Client.SimpleStation14.Skills;

[GenerateTypedNameReferences]
public sealed partial class SkillsMenu : FancyWindow
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly SharedSkillsSystem _skills = default!;

    private readonly SkillsBoundUserInterface _owner;

    public SkillsMenu(SkillsBoundUserInterface owner, SharedSkillsSystem skillsSystem)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _skills = skillsSystem;

        _owner = owner;

        UpdateSkills();
    }

    public void UpdateSkills()
    {
        var skills = _skills.GetAllSkillsCategorized();

        Skills.DisposeAllChildren();

        foreach (var category in skills.Keys)
        {
            var categoryLabel = new SkillsUIContainer();
            categoryLabel.SetHeading(category.Name);
            categoryLabel.SetDescription(category.Description);
            Skills.AddChild(categoryLabel);

            foreach (var subCategory in skills[category].Keys)
            {
                var subCategoryLabel = new SkillsUIContainer();
                subCategoryLabel.SetHeading(subCategory.Name);
                subCategoryLabel.SetDescription(subCategory.Description);
                subCategoryLabel.SetColor(subCategory.SubCategoryColor);
                Skills.AddChild(subCategoryLabel);

                foreach (var skill in skills[category][subCategory])
                {
                    var newLabel = new SkillsUIContainer();
                    newLabel.SetHeading(skill.Name);
                    newLabel.SetDescription(skill.Description);
                    Skills.AddChild(newLabel);
                }
            }
        }
    }
}
