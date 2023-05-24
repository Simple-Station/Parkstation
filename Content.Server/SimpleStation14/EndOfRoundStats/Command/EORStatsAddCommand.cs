using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Console;

namespace Content.Server.SimpleStation14.EndOfRoundStats.Command;

[AdminCommand(AdminFlags.Admin)]
public sealed class EORStatsAddCommmand : IConsoleCommand
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public string Command => "eorstatsadd";
    public string Description => "Adds an end of round stat to be displayed.";
    public string Help => $"Usage: {Command} <stat> <color?>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var _stats = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<CommandStatSystem>();

        if (args.Length < 1 || args.Length > 2)
        {
            shell.WriteError("Invalid amount of arguments.");
            return;
        }

        if (args.Length == 2 && !Color.TryFromName(args[1], out _))
        {
            shell.WriteError("Invalid color.");
            return;
        }

        _stats.eorStats.Add((args[0], args.Length == 2 ? args[1] : "Green"));

        shell.WriteLine($"Added {args[0]} to end of round stats.");

        _adminLogger.Add(LogType.AdminMessage, LogImpact.Low,
            $"{shell.Player!.Name} added '{args[0]}' to end of round stats.");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHint("<Stat>");
        }

        if (args.Length == 2)
        {
            var options = Color.GetAllDefaultColors().Select(o => new CompletionOption(o.Key));

            return CompletionResult.FromHintOptions(options, "<Color?>");
        }

        return CompletionResult.Empty;
    }
}
