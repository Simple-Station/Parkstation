using Content.Shared.Soul;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.AI.UI
{
    /// <summary>
    /// Initializes a <see cref="AICameraList"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class AICameraListBoundUserInterface : BoundUserInterface
    {
        private AICameraList _window = new AICameraList();

        public AICameraListBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window?.OpenCentered();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
    }
}
