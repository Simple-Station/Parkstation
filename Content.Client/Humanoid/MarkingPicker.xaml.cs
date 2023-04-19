using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Client.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Humanoid;

[GenerateTypedNameReferences]
public sealed partial class MarkingPicker : Control
{
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public Action<MarkingSet>? OnMarkingAdded;
    public Action<MarkingSet>? OnMarkingRemoved;
    public Action<MarkingSet>? OnMarkingColorChange;
    public Action<MarkingSet>? OnMarkingRankChange;

    private List<Color> _currentMarkingColors = new();

    private ItemList.Item? _selectedMarking;
    private ItemList.Item? _selectedUnusedMarking;
    private MarkingCategories _selectedMarkingCategory = MarkingCategories.Chest;

    private MarkingSet _currentMarkings = new();

    private List<MarkingCategories> _markingCategories = Enum.GetValues<MarkingCategories>().ToList();

    private string _currentSpecies = SharedHumanoidAppearanceSystem.DefaultSpecies;
    public Color CurrentSkinColor = Color.White;
    public Color CurrentEyeColor = Color.Black;
    public Marking? HairMarking;
    public Marking? FacialHairMarking;

    private readonly HashSet<MarkingCategories> _ignoreCategories = new();

    public string IgnoreCategories
    {
        get => string.Join(',',  _ignoreCategories);
        set
        {
            _ignoreCategories.Clear();
            var split = value.Split(',');
            foreach (var category in split)
            {
                if (!Enum.TryParse(category, out MarkingCategories categoryParse))
                {
                    continue;
                }

                _ignoreCategories.Add(categoryParse);
            }

            SetupCategoryButtons();
        }
    }

    public bool Forced { get; set; }
    // public bool Forced = true;

    private bool _ignoreSpecies;
    // private bool _ignoreSpecies = true;

    public bool IgnoreSpecies
    {
        get => _ignoreSpecies;
        set
        {
            _ignoreSpecies = value;
            // _ignoreSpecies = true;
            Populate(CMarkingSearch.Text);
        }
    }

    public void SetData(List<Marking> newMarkings, string species, Color skinColor, Color eyeColor)
    {
        var pointsProto = _prototypeManager
            .Index<SpeciesPrototype>(species).MarkingPoints;
        _currentMarkings = new(newMarkings, pointsProto, _markingManager);

        if (!IgnoreSpecies)
        {
            _currentMarkings.EnsureSpecies(species, skinColor, _markingManager); // should be validated server-side but it can't hurt
        }

        _currentSpecies = species;
        CurrentSkinColor = skinColor;
        CurrentEyeColor = eyeColor;

        Populate(CMarkingSearch.Text);
        PopulateUsed();
    }

    public void SetData(MarkingSet set, string species, Color skinColor, Color eyeColor)
    {
        _currentMarkings = set;

        if (!IgnoreSpecies)
        {
            _currentMarkings.EnsureSpecies(species, skinColor, _markingManager); // should be validated server-side but it can't hurt
        }

        _currentSpecies = species;
        CurrentSkinColor = skinColor;
        CurrentEyeColor = eyeColor;

        Populate(CMarkingSearch.Text);
        PopulateUsed();
    }

    public void SetSkinColor(Color color) => CurrentSkinColor = color;
    public void SetEyeColor(Color color) => CurrentEyeColor = color;

    public MarkingPicker()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        SetupCategoryButtons();
        CMarkingCategoryButton.OnItemSelected +=  OnCategoryChange;
        CMarkingsUnused.OnItemSelected += item =>
            _selectedUnusedMarking = CMarkingsUnused[item.ItemIndex];

        CMarkingAdd.OnPressed += args =>
            MarkingAdd();

        CMarkingsUsed.OnItemSelected += OnUsedMarkingSelected;

        CMarkingRemove.OnPressed += args =>
            MarkingRemove();

        CMarkingRankUp.OnPressed += _ => SwapMarkingUp();
        CMarkingRankDown.OnPressed += _ => SwapMarkingDown();

        CMarkingSearch.OnTextChanged += args => Populate(args.Text);
    }

    private void SetupCategoryButtons()
    {
        CMarkingCategoryButton.Clear();
        for (var i = 0; i < _markingCategories.Count; i++)
        {
            if (_ignoreCategories.Contains(_markingCategories[i]))
            {
                continue;
            }

            CMarkingCategoryButton.AddItem(Loc.GetString($"markings-category-{_markingCategories[i].ToString()}"), i);
        }
        CMarkingCategoryButton.SelectId(_markingCategories.IndexOf(_selectedMarkingCategory));
    }

    private string GetMarkingName(MarkingPrototype marking) => Loc.GetString($"marking-{marking.ID}");

    private List<string> GetMarkingStateNames(MarkingPrototype marking)
    {
        List<string> result = new();
        foreach (var markingState in marking.Sprites)
        {
            switch (markingState)
            {
                case SpriteSpecifier.Rsi rsi:
                    result.Add(Loc.GetString($"marking-{marking.ID}-{rsi.RsiState}"));
                    break;
                case SpriteSpecifier.Texture texture:
                    result.Add(Loc.GetString($"marking-{marking.ID}-{texture.TexturePath.Filename}"));
                    break;
            }
        }

        return result;
    }

    public void Populate(string filter)
    {
        CMarkingsUnused.Clear();
        _selectedUnusedMarking = null;

        var markings = IgnoreSpecies
            ? _markingManager.MarkingsByCategory(_selectedMarkingCategory)
            : _markingManager.MarkingsByCategoryAndSpecies(_selectedMarkingCategory, _currentSpecies);

        foreach (var marking in
            markings.Values.Where(m =>
                m.ID.ToLower().Contains(filter.ToLower()) ||
                GetMarkingName(m).ToLower().Contains(filter.ToLower())
            )
            .OrderBy(GetMarkingName)
            .ThenBy(m => m.ID)
        )
        {
            if (_currentMarkings.TryGetMarking(_selectedMarkingCategory, marking.ID, out _))
            {
                continue;
            }

            var item = CMarkingsUnused.AddItem($"{GetMarkingName(marking)}", marking.Sprites[0].Frame0());
            item.Metadata = marking;
        }

        CMarkingPoints.Visible = _currentMarkings.PointsLeft(_selectedMarkingCategory) != -1;
    }

    // Populate the used marking list. Returns a list of markings that weren't
    // valid to add to the marking list.
    public void PopulateUsed()
    {
        CMarkingsUsed.Clear();
        CMarkingColors.Visible = false;
        _selectedMarking = null;

        if (!IgnoreSpecies)
        {
            _currentMarkings.EnsureSpecies(_currentSpecies, null, _markingManager);
        }

        // walk backwards through the list for visual purposes
        foreach (var marking in _currentMarkings.GetReverseEnumerator(_selectedMarkingCategory))
        {
            if (!_markingManager.TryGetMarking(marking, out var newMarking))
            {
                continue;
            }

            var text = Loc.GetString(marking.Forced ? "marking-used-forced" : "marking-used", ("marking-name", $"{GetMarkingName(newMarking)}"),
                ("marking-category", Loc.GetString($"markings-category-{newMarking.MarkingCategory}")));

            var _item = new ItemList.Item(CMarkingsUsed)
            {
                Text = text,
                Icon = newMarking.Sprites[0].Frame0(),
                Selectable = true,
                Metadata = newMarking,
                IconModulate = marking.MarkingColors[0]
            };

            CMarkingsUsed.Add(_item);
        }

        // since all the points have been processed, update the points visually
        UpdatePoints();
    }

    private void SwapMarkingUp()
    {
        if (_selectedMarking == null)
        {
            return;
        }

        var i = CMarkingsUsed.IndexOf(_selectedMarking);
        if (ShiftMarkingRank(i, -1))
        {
            OnMarkingRankChange?.Invoke(_currentMarkings);
        }
    }

    private void SwapMarkingDown()
    {
        if (_selectedMarking == null)
        {
            return;
        }

        var i = CMarkingsUsed.IndexOf(_selectedMarking);
        if (ShiftMarkingRank(i, 1))
        {
            OnMarkingRankChange?.Invoke(_currentMarkings);
        }
    }

    private bool ShiftMarkingRank(int src, int places)
    {
        if (src + places >= CMarkingsUsed.Count || src + places < 0)
        {
            return false;
        }

        var visualDest = src + places; // what it would visually look like
        var visualTemp = CMarkingsUsed[visualDest];
        CMarkingsUsed[visualDest] = CMarkingsUsed[src];
        CMarkingsUsed[src] = visualTemp;

        switch (places)
        {
            // i.e., we're going down in rank
            case < 0:
                _currentMarkings.ShiftRankDownFromEnd(_selectedMarkingCategory, src);
                break;
            // i.e., we're going up in rank
            case > 0:
                _currentMarkings.ShiftRankUpFromEnd(_selectedMarkingCategory, src);
                break;
            // do nothing?
            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }

        return true;
    }



    // repopulate in case markings are restricted,
    // and also filter out any markings that are now invalid
    // attempt to preserve any existing markings as well:
    // it would be frustrating to otherwise have all markings
    // cleared, imo
    public void SetSpecies(string species)
    {
        _currentSpecies = species;
        var markingList = _currentMarkings.GetForwardEnumerator().ToList();

        var speciesPrototype = _prototypeManager.Index<SpeciesPrototype>(species);

        _currentMarkings = new(markingList, speciesPrototype.MarkingPoints, _markingManager, _prototypeManager);
        _currentMarkings.EnsureSpecies(species, null, _markingManager);

        Populate(CMarkingSearch.Text);
        PopulateUsed();
    }

    private void UpdatePoints()
    {
        var count = _currentMarkings.PointsLeft(_selectedMarkingCategory);
        if (count > -1)
        {
            CMarkingPoints.Text = Loc.GetString("marking-points-remaining", ("points", count));
        }
    }

    private void OnCategoryChange(OptionButton.ItemSelectedEventArgs category)
    {
        CMarkingCategoryButton.SelectId(category.Id);
        _selectedMarkingCategory = _markingCategories[category.Id];
        Populate(CMarkingSearch.Text);
        PopulateUsed();
        UpdatePoints();
    }

    // TODO: This should be using ColorSelectorSliders once that's merged, so
    private void OnUsedMarkingSelected(ItemList.ItemListSelectedEventArgs item)
    {
        _selectedMarking = CMarkingsUsed[item.ItemIndex];
        var prototype = (MarkingPrototype) _selectedMarking.Metadata!;

        if (prototype.ForcedColoring)
        {
            CMarkingColors.Visible = false;

            return;
        }

        var stateNames = GetMarkingStateNames(prototype);
        _currentMarkingColors.Clear();
        CMarkingColors.DisposeAllChildren();
        List<ColorSelectorSliders> colorSliders = new();
        for (int i = 0; i < prototype.Sprites.Count; i++)
        {
            var colorContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
            };

            CMarkingColors.AddChild(colorContainer);

            ColorSelectorSliders colorSelector = new ColorSelectorSliders();
            colorSliders.Add(colorSelector);

            colorContainer.AddChild(new Label { Text = $"{stateNames[i]} color:" });
            colorContainer.AddChild(colorSelector);

            var listing = _currentMarkings.Markings[_selectedMarkingCategory];

            var color = listing[listing.Count - 1 - item.ItemIndex].MarkingColors[i];
            var currentColor = new Color(
                color.RByte,
                color.GByte,
                color.BByte
            );
            colorSelector.Color = currentColor;
            _currentMarkingColors.Add(currentColor);
            var colorIndex = _currentMarkingColors.Count - 1;

            Action<Color> colorChanged = _ =>
            {
                _currentMarkingColors[colorIndex] = colorSelector.Color;

                ColorChanged(colorIndex);
            };
            colorSelector.OnColorChanged += colorChanged;
        }

        CMarkingColors.Visible = true;
    }

    private void ColorChanged(int colorIndex)
    {
        if (_selectedMarking is null) return;
        var markingPrototype = (MarkingPrototype) _selectedMarking.Metadata!;
        int markingIndex = _currentMarkings.FindIndexOf(_selectedMarkingCategory, markingPrototype.ID);

        if (markingIndex < 0) return;

        _selectedMarking.IconModulate = _currentMarkingColors[colorIndex];

        var marking = new Marking(_currentMarkings.Markings[_selectedMarkingCategory][markingIndex]);
        marking.SetColor(colorIndex, _currentMarkingColors[colorIndex]);
        _currentMarkings.Replace(_selectedMarkingCategory, markingIndex, marking);

        OnMarkingColorChange?.Invoke(_currentMarkings);
    }

    private void MarkingAdd()
    {
        if (_selectedUnusedMarking is null) return;

        if (_currentMarkings.PointsLeft(_selectedMarkingCategory) == 0 && !Forced)
        {
            return;
        }

        var marking = (MarkingPrototype) _selectedUnusedMarking.Metadata!;
        var markingObject = marking.AsMarking();

        // We need add hair markings in cloned set manually because _currentMarkings doesn't have it
        var markingSet = new MarkingSet(_currentMarkings);
        if (HairMarking != null)
        {
            markingSet.AddBack(MarkingCategories.Hair, HairMarking);
        }
        if (FacialHairMarking != null)
        {
            markingSet.AddBack(MarkingCategories.FacialHair, FacialHairMarking);
        }

        if (!_markingManager.MustMatchSkin(_currentSpecies, marking.BodyPart, out var _, _prototypeManager))
        {
            // Do default coloring
            var colors = MarkingColoring.GetMarkingLayerColors(
                marking,
                CurrentSkinColor,
                CurrentEyeColor,
                markingSet
            );
            for (var i = 0; i < colors.Count; i++)
            {
                markingObject.SetColor(i, colors[i]);
            }
        }
        else
        {
            // Color everything in skin color
            for (var i = 0; i < marking.Sprites.Count; i++)
            {
                markingObject.SetColor(i, CurrentSkinColor);
            }
        }

        markingObject.Forced = Forced;

        _currentMarkings.AddBack(_selectedMarkingCategory, markingObject);

        UpdatePoints();

        CMarkingsUnused.Remove(_selectedUnusedMarking);
        var item = new ItemList.Item(CMarkingsUsed)
        {
            Text = Loc.GetString("marking-used", ("marking-name", $"{GetMarkingName(marking)}"), ("marking-category", Loc.GetString($"markings-category-{marking.MarkingCategory}"))),
            Icon = marking.Sprites[0].Frame0(),
            Selectable = true,
            Metadata = marking,
        };
        CMarkingsUsed.Insert(0, item);

        _selectedUnusedMarking = null;
        OnMarkingAdded?.Invoke(_currentMarkings);
    }

    private void MarkingRemove()
    {
        if (_selectedMarking is null) return;

        var marking = (MarkingPrototype) _selectedMarking.Metadata!;

        _currentMarkings.Remove(_selectedMarkingCategory, marking.ID);

        UpdatePoints();

        CMarkingsUsed.Remove(_selectedMarking);

        if (marking.MarkingCategory == _selectedMarkingCategory)
        {
            var item = CMarkingsUnused.AddItem($"{GetMarkingName(marking)}", marking.Sprites[0].Frame0());
            item.Metadata = marking;
        }
        _selectedMarking = null;
        CMarkingColors.Visible = false;
        OnMarkingRemoved?.Invoke(_currentMarkings);
    }
}
