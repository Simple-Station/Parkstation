using Content.Shared.SimpleStation14.Jukebox;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.SimpleStation14.Prototypes;

namespace Content.Client.SimpleStation14.Jukebox.Ui;

[UsedImplicitly]
public sealed class JukeboxBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entity = default!;

    private JukeboxWindow? _window;

    public JukeboxBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base (owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (!_entity.TryGetComponent<JukeboxComponent>(Owner.Owner, out var jukeboxComp))
        {
            Logger.Error($"No Jukebox component found for {_entity.ToPrettyString(Owner.Owner)}!");
            return;
        }

        _window = new JukeboxWindow(this, Owner.Owner, jukeboxComp)
        {
            Title = _entity.GetComponent<MetaDataComponent>(Owner.Owner).EntityName
        };

        _window.OpenCentered();
        _window.OnClose += Close;

        _window.OnPlayButtonPressed += () => SendMessage(new JukeboxPlayButtonPressedMessage());

        _window.OnSkipButtonPressed += () => SendMessage(new JukeboxSkipButtonPressedMessage());

        _window.OnSongSelected += song => SendMessage(new JukeboxSongSelectedMessage(song));
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage msg)
    {
        base.ReceiveMessage(msg);

        if (msg is not JukeboxUpdateStateMessage jukeboxMessage)
            return;

        _window?.UpdateState(jukeboxMessage.PopulateSongs);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _window?.Dispose();
    }
}

