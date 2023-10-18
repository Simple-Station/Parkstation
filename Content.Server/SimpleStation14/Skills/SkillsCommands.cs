using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.Players;
using Robust.Server.Player;
using Robust.Server.GameObjects;
using Content.Shared.SimpleStation14.Skills.Systems;

namespace Content.Server.SimpleStation14.Skills;

// [AdminCommand(AdminFlags.Logs)]
// public sealed class SkillUiCommand : IConsoleCommand
// {
//     public string Command => "skillui";
//     public string Description => Loc.GetString("command-skillui-description");
//     public string Help => Loc.GetString("command-skillui-help");

//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var entityManager = IoCManager.Resolve<IEntityManager>();
//         var uiSystem = IoCManager.Resolve<UserInterfaceSystem>();
//         var player = shell.Player as IPlayerSession;
//         EntityUid? entity = null;

//         if (args.Length == 0 && player != null)
//         {
//             entity = player.ContentData()?.Mind?.CurrentEntity;
//         }
//         else if (IoCManager.Resolve<IPlayerManager>().TryGetPlayerDataByUsername(args[0], out var data))
//         {
//             entity = data.ContentData()?.Mind?.CurrentEntity;
//         }
//         else if (EntityUid.TryParse(args[0], out var foundEntity))
//         {
//             entity = foundEntity;
//         }

//         if (entity == null)
//         {
//             shell.WriteLine("Can't find entity.");
//             return;
//         }

//         if (!uiSystem.TryGetUi(entity.Value, SkillsUiKey.Key, out _))
//             return;

//         uiSystem.TryOpen(entity.Value, SkillsUiKey.Key, player!);
//     }
// }

[AdminCommand(AdminFlags.Logs)]
public sealed class SkillModifyCommand : IConsoleCommand
{
    public string Command => "skillmodify";
    public string Description => Loc.GetString("command-skillmodify-description");
    public string Help => Loc.GetString("command-skillmodify-help");

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var player = shell.Player as IPlayerSession;
        EntityUid? entity = null;
        var skillSystem = entityManager.EntitySysManager.GetEntitySystem<SharedSkillsSystem>();

        if (args.Length < 3 && player != null)
        {
            entity = player.ContentData()?.Mind?.CurrentEntity;
        }
        else if (IoCManager.Resolve<IPlayerManager>().TryGetPlayerDataByUsername(args[2], out var data))
        {
            entity = data.ContentData()?.Mind?.CurrentEntity;
        }
        else if (EntityUid.TryParse(args[2], out var foundEntity))
        {
            entity = foundEntity;
        }

        if (entity == null)
        {
            shell.WriteLine("Can't find entity.");
            return;
        }

        if (!skillSystem.TryModifySkillLevel(entity.Value, args[0], int.Parse(args[1])))
            shell.WriteLine("Failed to modify skill.");
    }
}
