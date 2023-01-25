using Content.Shared.Administration;
using Content.Server.Tips;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Debug)]
    public sealed class ShowTipCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "showtip";
        public string Description => "Shows a random tip";
        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var _tipsSystem = _entities.EntitySysManager.GetEntitySystem<TipsSystem>();
            _tipsSystem.AnnounceRandomTip();
            shell.WriteLine("Sent a random tip.");
        }
    }
}
