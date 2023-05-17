using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.SimpleStation14.Examine.SpriteExamine.UI;

[GenerateTypedNameReferences]
public sealed partial class SpriteExamineWindow : FancyWindow
{
    private readonly IEntityManager _entity;

    // ReSharper disable once InconsistentNaming
    private GridContainer _sprites => SpriteContainer;
    // ReSharper disable once InconsistentNaming
    private Label _name => Name;
    // ReSharper disable once InconsistentNaming
    private Label _job => Job;
    // ReSharper disable once InconsistentNaming
    private RichTextLabel _flavor => FlavorText;

    public SpriteExamineWindow()
    {
        RobustXamlLoader.Load(this);

        _entity = IoCManager.Resolve<IEntityManager>();

        ResetUi();
    }


    private void ResetUi()
    {
        _sprites.RemoveAllChildren();

        _name.Text = "Placeholder Name";
        _job.Text = "Placeholder Job";

        // TODO: Localization
        _flavor.SetMessage(Loc.GetString("sprite-examine-window-flavor-text-placeholder"));
    }

    /// <summary>
    ///     Updates the UI to show all relevant information about the entity
    /// </summary>
    /// <param name="examined">The entity to become informed about</param>
    /// <param name="name">The name of the examined entity, taken from their ID</param>
    /// <param name="job">The job of the examined entity, taken from their ID</param>
    /// <param name="flavorText">The flavor text of the examined entity</param>
    public void UpdateUi(EntityUid examined, string? name = null, string? job = null, string? flavorText = null)
    {
        ResetUi();

        // Fill in the omnidirectional sprite views
        if (_entity.TryGetComponent<SpriteComponent>(examined, out var sprite))
        {
            FillSprites(sprite);
        }

        // Fill in the name and job
        _name.Text = name;
        _job.Text = job;

        // Fill in the flavor text
        if (!string.IsNullOrEmpty(flavorText))
            _flavor.SetMessage(flavorText);
    }


    /// <summary>
    ///     Fills the sprite views with the sprite from the sprite component
    /// </summary>
    /// <param name="sprite">Sprite component to use</param>
    private void FillSprites(SpriteComponent sprite)
    {
        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.South,
            Margin = new Thickness(0, 0, 8, 8),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.North,
            Margin = new Thickness(8, 0, 0, 8),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.West,
            Margin = new Thickness(0, 8, 8, 0),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.East,
            Margin = new Thickness(8, 8, 0, 0),
        });
    }
}
