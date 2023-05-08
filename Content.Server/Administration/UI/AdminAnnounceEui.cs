using Content.Server.Administration.Managers;
using Content.Server.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.SimpleStation14.Announcements.Systems;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.UI
{
    public sealed class AdminAnnounceEui : BaseEui
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        private readonly AnnouncerSystem _announcerSystem;
        private readonly ChatSystem _chatSystem;

        public AdminAnnounceEui()
        {
            IoCManager.InjectDependencies(this);
            _chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
            _announcerSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<AnnouncerSystem>();
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new AdminAnnounceEuiState();
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case AdminAnnounceEuiMsg.DoAnnounce doAnnounce:
                    if (!_adminManager.HasAdminFlag(Player, AdminFlags.Admin))
                    {
                        Close();
                        break;
                    }

                    switch (doAnnounce.AnnounceType)
                    {
                        case AdminAnnounceType.Server:
                            _chatManager.DispatchServerAnnouncement(doAnnounce.Announcement);
                            break;
                        // TODO: Per-station announcement support
                        case AdminAnnounceType.Station:
                            // _chatSystem.DispatchGlobalAnnouncement(doAnnounce.Announcement, doAnnounce.Announcer, colorOverride: Color.Gold);
                            var tmp = _announcerSystem.Announcer;
                            _proto.TryIndex<AnnouncerPrototype>(doAnnounce.AnnouncerVoice, out var announcer);
                            _announcerSystem.Announcer = announcer ?? tmp;

                            _announcerSystem.SendAnnouncement(string.IsNullOrEmpty(doAnnounce.AnnouncerSound) ? "commandreport" : doAnnounce.AnnouncerSound,
                                Filter.Broadcast(), doAnnounce.Announcement, doAnnounce.Announcer, Color.Gold);

                            _announcerSystem.Announcer = tmp;
                            break;
                    }

                    StateDirty();

                    if (doAnnounce.CloseAfter)
                        Close();

                    break;
            }
        }
    }
}
