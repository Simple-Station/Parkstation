using System.Linq;
using Content.Server.Administration;
using Content.Server.SimpleStation14.Announcements.Systems;
using Content.Shared.Administration;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Announcements
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AnnounceCommand : IConsoleCommand
    {
        public string Command => "announce";
        public string Description => "Send an in-game announcement.";
        public string Help => $"{Command} <sender> <message> <sound> <announcer voice>";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
            var announce = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<AnnouncerSystem>();
            var proto = IoCManager.Resolve<IPrototypeManager>();

            switch (args.Length)
            {
                case 0:
                    shell.WriteError("Not enough arguments! Need at least 1.");
                    return;
                case 1:
                    // chat.DispatchGlobalAnnouncement(args[0], colorOverride: Color.Gold);
                    announce.SendAnnouncement("commandreport", Filter.Broadcast(), args[0], "Central Command", Color.Gold);
                    break;
                case 2:
                    // chat.DispatchGlobalAnnouncement(message, args[0], colorOverride: Color.Gold);
                    announce.SendAnnouncement("commandreport", Filter.Broadcast(), args[1], args[0], Color.Gold);
                    break;
                case 3:
                    announce.SendAnnouncement(args[2], Filter.Broadcast(), args[1], args[0], Color.Gold);
                    break;
                case 4:
                    var old = announce.Announcer;
                    if (!proto.TryIndex(args[3], out AnnouncerPrototype? prototype))
                    {
                        shell.WriteError($"No announcer prototype with ID {args[3]} found!");
                        return;
                    }
                    announce.Announcer = prototype;
                    announce.SendAnnouncement(args[2], Filter.Broadcast(), args[1], args[0], Color.Gold);
                    announce.Announcer = old;
                    break;
            }

            shell.WriteLine("Sent!");
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 3)
            {
                var list = new List<string>();
                foreach (var prototype in IoCManager.Resolve<IPrototypeManager>().EnumeratePrototypes<AnnouncerPrototype>()
                    .SelectMany(x => x.AnnouncementPaths.Select(x => x.ID)))
                {
                    list.Add(prototype);
                }
                return CompletionResult.FromHintOptions(list, Loc.GetString("admin-announce-hint-sound"));
            }

            if (args.Length == 4)
            {
                var list = new List<string>();
                foreach (var prototype in IoCManager.Resolve<IPrototypeManager>().EnumeratePrototypes<AnnouncerPrototype>())
                {
                    list.Add(prototype.ID);
                }
                return CompletionResult.FromHintOptions(list, Loc.GetString("admin-announce-hint-voice"));
            }

            return CompletionResult.Empty;
        }
    }
}
