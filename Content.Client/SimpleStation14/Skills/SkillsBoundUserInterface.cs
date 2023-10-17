using JetBrains.Annotations;
using Content.Shared.Borgs;
using Robust.Client.GameObjects;
using Content.Shared.SimpleStation14.Skills.Systems;

namespace Content.Client.SimpleStation14.Skills;

[UsedImplicitly]
public sealed class SkillsBoundUserInterface : BoundUserInterface
{
    private SkillsMenu? _skillsMenu;

    public SkillsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _skillsMenu = new SkillsMenu(this, EntMan.System<SharedSkillsSystem>());

        _skillsMenu.OnClose += Close;

        _skillsMenu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _skillsMenu?.Dispose();
    }
}
