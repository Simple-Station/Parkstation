using Content.Client.SimpleStation14.Documentation;
using Content.Shared.Administration;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Client.Commands
{
    [AnyCommand]
    public sealed class OpenDocumentationCommand : IConsoleCommand
    {
        public string Command => "opendocumentation";
        public string Description => $"Opens the in-game documentation UI.";
        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // open the ui or something, i dont know
        }
    }
}
