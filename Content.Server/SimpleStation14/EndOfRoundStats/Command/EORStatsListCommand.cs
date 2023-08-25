using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Command;

[AdminCommand(AdminFlags.Admin)]
public sealed class EORStatsCommand : IConsoleCommand
{
    public string Command => "eorstatslist";
    public string Description => "Lists the current command-added end of round stats to be displayed.";
    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var _stats = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<CommandStatSystem>();

        if (args.Length != 0)
        {
            shell.WriteError("Invalid amount of arguments.");
            return;
        }

        if (_stats.eorStats.Count == 0)
        {
            shell.WriteLine("No command-added end of round stats to display.");
            return;
        }

        shell.WriteLine("End of round stats:");

        foreach (var (stat, color) in _stats.eorStats)
        {
            shell.WriteLine($"'{stat}' - {color}");
        }
    }
}
