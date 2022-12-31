using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Content.Client.SimpleStation14.Documentation.UI;

namespace Content.Client.SimpleStation14.Documentation.UI.Pages
{
    [GenerateTypedNameReferences]
    public sealed partial class WikiPages : Control
    {
        public BoxContainer categories => Categories;
        public BoxContainer contents => Contents;

        public WikiPages()
        {
            RobustXamlLoader.Load(this);

            foreach (Button? button in categories.Children)
            {
                if (button != null)
                {
                    button.OnPressed += ev =>
                    {
                        if (ev.Button.Name != null)
                        {
                            SetPage(ev.Button);
                        }
                    };
                }
            }
        }

        public void SetPage(BaseButton page)
        {
            foreach (var child in contents.Children)
            {
                if (child.Name == page.Name + "Content")
                {
                    // Need to find a way to edit the DocsWindow that is currently active
                }
            }
        }
    }
}
