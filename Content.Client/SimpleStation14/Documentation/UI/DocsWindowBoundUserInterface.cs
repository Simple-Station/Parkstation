using Content.Shared.SimpleStation14.Documentation;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.Documentation.UI
{
    public sealed class DocsWindowBoundUserInterface : BoundUserInterface
    {
        private DocsWindow? _window;

        public DocsWindowBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey) { }

        protected override void Open()
        {
            base.Open();

            _window = new DocsWindow();
            if (State != null)
                UpdateState(State);

            _window.OpenCentered();

            _window.OnClose += Close;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
    }
}
