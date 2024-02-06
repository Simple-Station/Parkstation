using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.SimpleStation14.Holograms;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class AcceptHologramEui : BaseEui
{
    private readonly HologramSystem _hologramSystem;
    private readonly Mind.Mind _mind;

    public AcceptHologramEui(Mind.Mind mind, HologramSystem hologramSys)
    {
        _mind = mind;
        _hologramSystem = hologramSys;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AcceptHologramChoiceMessage choice ||
            choice.Button == AcceptHologramUiButton.Deny)
        {
            Close();
            return;
        }

        _hologramSystem.TransferMindToHologram(_mind);
        Close();
    }
}
