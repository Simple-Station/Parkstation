using Content.Server.Administration;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Server.SimpleStation14.Announcements.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Announcements
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AnnounceCommand : IConsoleCommand
    {
        public string Command => "announce";
        public string Description => "Send an in-game announcement.";
        public string Help => $"{Command} <sender> <message> or {Command} <message> to send announcement as CentCom.";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
            var announce = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<AnnouncerSystem>();

            if (args.Length == 0)
            {
                shell.WriteError("Not enough arguments! Need at least 1.");
                return;
            }

            if (args.Length == 1)
            {
                // chat.DispatchGlobalAnnouncement(args[0], colorOverride: Color.Gold);
                announce.SendAnnouncement("announce", Filter.Broadcast(), args[0], "Central Command", Color.Gold);
            }
            else
            {
                var message = string.Join(' ', new ArraySegment<string>(args, 1, args.Length-1));
                // chat.DispatchGlobalAnnouncement(message, args[0], colorOverride: Color.Gold);
                announce.SendAnnouncement("announce", Filter.Broadcast(), message, args[0], Color.Gold);
            }
            shell.WriteLine("Sent!");
        }
    }
}
