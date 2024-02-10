using Content.Shared.SimpleStation14.Holograms;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.Holograms.CctvDatabaseUi;

public sealed class CctvDatabaseBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CctvDatabaseWindow? _menu;

    public CctvDatabaseBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        _menu = new CctvDatabaseWindow();

        _menu.OpenCentered();
        _menu.OnClose += Close;

        _menu.PrintRequested += SendPrintRequest;
    }

    private void SendPrintRequest(int index)
    {
        Logger.Error($"Sending message for index {index}");
        SendMessage(new CctvDatabasePrintRequestMessage(index));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CctvDatabaseState cctvState:
                _menu?.UpdateState(cctvState);
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
