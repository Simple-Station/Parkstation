using Content.Shared.SimpleStation14.Jukebox;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.Jukebox.Ui;

[UsedImplicitly]
public sealed class JukeboxBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entity = default!;
    private readonly ISawmill _log = default!;

    private JukeboxWindow? _window;

    public JukeboxBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _log = EntMan.System<SharedJukeBoxSystem>().Log;
    }

    protected override void Open()
    {
        base.Open();

        if (!_entity.TryGetComponent<JukeboxComponent>(Owner, out var jukeboxComp))
        {
            _log.Error("No Jukebox component found for {0}!", _entity.ToPrettyString(Owner));
            return;
        }

        _window = new JukeboxWindow(jukeboxComp, _log)
        {
            Title = _entity.GetComponent<MetaDataComponent>(Owner).EntityName
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

