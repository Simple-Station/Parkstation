using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Shared.Administration;
using Content.Shared.GameTicking;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Commands;

[AnyCommand]
public sealed class GhostRespawnCommand : IConsoleCommand
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public string Command => "ghostrespawn";
    public string Description => Loc.GetString("ghost-respawn-command-desc");
    public string Help => Command;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is null)
        {
            shell.WriteLine(Loc.GetString("ghost-respawn-not-a-player"));
            return;
        }

        if (!_configurationManager.GetCVar(SimpleStationCCVars.RespawnEnabled))
        {
            shell.WriteLine(Loc.GetString("ghost-respawn-disabled"));
            return;
        }

        if (shell.Player.AttachedEntity is null)
        {
            shell.WriteLine(Loc.GetString("ghost-respawn-not-a-ghost"));
            if (_gameTicker.PlayerGameStatuses[shell.Player.UserId] == PlayerGameStatus.JoinedGame)
            {
                shell.WriteLine(Loc.GetString("ghost-respawn-not-a-ghost-exception"));
                _gameTicker.Respawn((IPlayerSession) shell.Player);
            }
            return;
        }

        if (!_entityManager.TryGetComponent<GhostComponent>(shell.Player.AttachedEntity, out var ghost))
        {
            shell.WriteLine(Loc.GetString("ghost-respawn-not-a-ghost"));
            return;
        }

        _entityManager.Dirty(ghost);

        var time = (_gameTiming.CurTime - ghost.TimeOfDeath);
        var respawnTime = _configurationManager.GetCVar(SimpleStationCCVars.RespawnTime);

        if (respawnTime > time.TotalSeconds)
        {
            shell.WriteLine(Loc.GetString("ghost-respawn-not-dead-long-enough", ("time", time.TotalSeconds), ("respawnTime", respawnTime)));
            return;
        }

        _gameTicker.Respawn((IPlayerSession) shell.Player);
    }
}
