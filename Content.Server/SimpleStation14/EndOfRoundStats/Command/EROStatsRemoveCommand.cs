using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Command;

[AdminCommand(AdminFlags.Admin)]
public sealed class EORStatsRemoveCommand : IConsoleCommand
{
    public string Command => "eorstatsremove";
    public string Description => "Removes a previously added end of round stat. Defaults to last added stat.";
    public string Help => $"Usage: {Command} <stat index?>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var _stats = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<CommandStatSystem>();

        if (args.Length > 1)
        {
            shell.WriteError("Invalid amount of arguments.");
            return;
        }

        if (_stats.eorStats.Count == 0)
        {
            shell.WriteError("No stats to remove.");
            return;
        }

        int index = _stats.eorStats.Count;

        if (args.Length == 1)
        {
            if (!int.TryParse(args[0], out index))
            {
                shell.WriteError("Invalid index.");
                return;
            }

            if (index < 0 || index > _stats.eorStats.Count)
            {
                shell.WriteError("Index out of range.");
                return;
            }
        }

        index--;

        shell.WriteLine($"Removed '{_stats.eorStats[index].Item1}' from end of round stats.");

        _stats.eorStats.RemoveAt(index);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var _stats = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<CommandStatSystem>();

        if (args.Length == 1)
        {
            var options = _stats.eorStats.Select(o => new CompletionOption
                ((_stats.eorStats.LastIndexOf((o.Item1, o.Item2)) + 1).ToString(), o.Item1));

            if (options.Count() == 0)
                return CompletionResult.FromHint("No stats to remove.");

            return CompletionResult.FromOptions(options);
        }

        return CompletionResult.Empty;
    }
}
