using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Holograms;

public sealed class AcceptHologramWindow : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;

    public AcceptHologramWindow()
    {

        Title = Loc.GetString("accept-hologram-window-title");

        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    Children =
                    {
                        new Label()
                        {
                            Text = Loc.GetString("accept-hologram-window-prompt-text-part")
                        },
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (AcceptButton = new Button
                                {
                                    Text = Loc.GetString("accept-hologram-window-accept-button"),
                                }),

                                new Control()
                                {
                                    MinSize = new Vector2(20, 0)
                                },

                                (DenyButton = new Button
                                {
                                    Text = Loc.GetString("accept-hologram-window-deny-button"),
                                })
                            }
                        },
                    }
                },
            }
        });
    }
}
