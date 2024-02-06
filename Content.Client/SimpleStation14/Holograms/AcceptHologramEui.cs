using Content.Client.Eui;
using Content.Client.Holograms;
using Content.Shared.SimpleStation14.Holograms;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.SimpleStation14.Holograms;

[UsedImplicitly]
public sealed class AcceptHologramEui : BaseEui
{
    private readonly AcceptHologramWindow _window;

    public AcceptHologramEui()
    {
        _window = new AcceptHologramWindow();

        _window.DenyButton.OnPressed += _ =>
        {
            SendMessage(new AcceptHologramChoiceMessage(AcceptHologramUiButton.Deny));
            _window.Close();
        };

        _window.OnClose += () => SendMessage(new AcceptHologramChoiceMessage(AcceptHologramUiButton.Deny));

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new AcceptHologramChoiceMessage(AcceptHologramUiButton.Accept));
            _window.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

}
