using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Console;

namespace Content.Server.SimpleStation14.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AdminvisCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public string Command => "adminvis";
        public string Description => "Makes you only visible to ghosts, or visible to everyone.";
        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is null)
            {
                shell.WriteLine("You cannot run this from the console!");
                return;
            }

            if (shell.Player.AttachedEntity is null)
            {
                shell.WriteLine("You cannot run this in the lobby, or without an entity.");
                return;
            }

            var visComp = _entityManager.EnsureComponent<VisibilityComponent>((EntityUid) shell.Player.AttachedEntity);
            var _visibilitySystem = EntitySystem.Get<VisibilitySystem>();
            var currentLayer = visComp.Layer;

            if (currentLayer == 1) currentLayer = 2;
            else currentLayer = 1;

            _visibilitySystem.SetLayer(visComp, currentLayer);
            var visString = currentLayer == 1 ? "visible" : "invisible";
            shell.WriteLine($"Set visibility to {visString}.");
        }
    }
}
