using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Borgs;
using Robust.Shared.Console;
using Content.Server.Players;
using Robust.Server.Player;
using Robust.Server.GameObjects;
using Content.Shared.SimpleStation14.Skills.Systems;

namespace Content.Server.SimpleStation14.Skills;

[AdminCommand(AdminFlags.Logs)]
public sealed class SkillUiCommand : IConsoleCommand
{
    public string Command => "skillui";
    public string Description => Loc.GetString("command-skillui-description");
    public string Help => Loc.GetString("command-skillui-help");

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var uiSystem = IoCManager.Resolve<UserInterfaceSystem>();
        var player = shell.Player as IPlayerSession;
        EntityUid? entity = null;

        if (args.Length == 0 && player != null)
        {
            entity = player.ContentData()?.Mind?.CurrentEntity;
        }
        else if (IoCManager.Resolve<IPlayerManager>().TryGetPlayerDataByUsername(args[0], out var data))
        {
            entity = data.ContentData()?.Mind?.CurrentEntity;
        }
        else if (EntityUid.TryParse(args[0], out var foundEntity))
        {
            entity = foundEntity;
        }

        if (entity == null)
        {
            shell.WriteLine("Can't find entity.");
            return;
        }

        if (!uiSystem.TryGetUi(entity.Value, SkillsUiKey.Key, out _))
            return;

        uiSystem.TryOpen(entity.Value, SkillsUiKey.Key, player!);
    }
}

// [AdminCommand(AdminFlags.Logs)]
// public sealed class ListSkillsCommand : IConsoleCommand
// {
//     public string Command => "skillsls";
//     public string Description => Loc.GetString("command-skillsls-description");
//     public string Help => Loc.GetString("command-skillsls-help");

//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var entityManager = IoCManager.Resolve<IEntityManager>();
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

//         if (!entityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
//         {
//             shell.WriteLine("Entity has no skills.");
//             return;
//         }

//         // Parkstation-Skills-Start
//         shell.WriteLine($"Name: {Loc.GetString($"skillset-name-{skills.SkillsID}")}");
//         shell.WriteLine($"Description: {Loc.GetString($"skillset-description-{skills.SkillsID}")}");
//         // Parkstation-Skills-End

//         shell.WriteLine($"Skills for {entityManager.ToPrettyString(entity.Value)}:");
//         foreach (var skills in skills.Skills)
//         {
//             shell.WriteLine(skills);
//         }
//     }
// }

// [AdminCommand(AdminFlags.Fun)]
// public sealed class ClearSkillsCommand : IConsoleCommand
// {
//     public string Command => "skillsclear";
//     public string Description => Loc.GetString("command-skillsclear-description");
//     public string Help => Loc.GetString("command-skillsclear-help");
//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var entityManager = IoCManager.Resolve<IEntityManager>();
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

//         if (!entityManager.TryGetComponent<SkillsComponent>(entity.Value, out var skills))
//         {
//             shell.WriteLine("Entity has no skills component to clear");
//             return;
//         }

//         entityManager.EntitySysManager.GetEntitySystem<SkillsSystem>().ClearSkills(entity.Value, skills);
//     }
// }

// [AdminCommand(AdminFlags.Fun)]
// public sealed class AddSkillsCommand : IConsoleCommand
// {
//     public string Command => "skillsadd";
//     public string Description => Loc.GetString("command-skillsadd-description");
//     public string Help => Loc.GetString("command-skillsadd-help");
//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var entityManager = IoCManager.Resolve<IEntityManager>();
//         var player = shell.Player as IPlayerSession;
//         EntityUid? entity = null;

//         if (args.Length < 2 || args.Length > 3)
//         {
//             shell.WriteLine("Wrong number of arguments.");
//             return;
//         }

//         if (IoCManager.Resolve<IPlayerManager>().TryGetPlayerDataByUsername(args[0], out var data))
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

//         var skills = entityManager.EnsureComponent<SkillsComponent>(entity.Value);

//         if (args.Length == 2)
//             entityManager.EntitySysManager.GetEntitySystem<SkillsSystem>().AddSkills(entity.Value, args[1], component: skills);
//         else if (args.Length == 3 && int.TryParse(args[2], out var index))
//             entityManager.EntitySysManager.GetEntitySystem<SkillsSystem>().AddSkills(entity.Value, args[1], index, skills);
//         else
//             shell.WriteLine("Third argument must be an integer.");
//     }
// }

// [AdminCommand(AdminFlags.Fun)]
// public sealed class RemoveSkillsCommand : IConsoleCommand
// {
//     public string Command => "skillsrm";
//     public string Description => Loc.GetString("command-skillsrm-description");
//     public string Help => Loc.GetString("command-skillsrm-help");
//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var entityManager = IoCManager.Resolve<IEntityManager>();
//         var player = shell.Player as IPlayerSession;
//         EntityUid? entity = null;

//         if (args.Length < 1 || args.Length > 2)
//         {
//             shell.WriteLine("Wrong number of arguments.");
//             return;
//         }

//         if (IoCManager.Resolve<IPlayerManager>().TryGetPlayerDataByUsername(args[0], out var data))
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

//         if (!entityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
//         {
//             shell.WriteLine("Entity has no skills to remove!");
//             return;
//         }

//         if (args[1] == null || !int.TryParse(args[1], out var index))
//             entityManager.EntitySysManager.GetEntitySystem<SkillsSystem>().RemoveSkills(entity.Value);
//         else
//             entityManager.EntitySysManager.GetEntitySystem<SkillsSystem>().RemoveSkills(entity.Value, index);
//     }
// }
