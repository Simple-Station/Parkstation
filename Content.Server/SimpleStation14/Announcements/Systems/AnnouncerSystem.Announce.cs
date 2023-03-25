using System.Linq;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.SimpleStation14.Announcements.Systems
{
    public sealed partial class AnnouncerSystem : EntitySystem
    {
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;

        /// <summary>
        ///     Gets an announcement path from the announcer
        /// </summary>
        public string GetAnnouncementPath(string announcementId)
        {
            var announcement = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId);
            if (announcement == null) announcement = Announcer.AnnouncementPaths.First(a => a.ID == "fallback");

            return announcement.Path;
        }

        /// <summary>
        ///     Gets an announcement SoundSpecifier from the announcer
        /// </summary>
        public SoundSpecifier GetAnnouncementSpecifier(string announcementId)
        {
            return new SoundPathSpecifier(GetAnnouncementPath(announcementId));
        }


        /// <summary>
        ///     Sends an announcement
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter, AudioParams? audioParams = null)
        {
            string announcement = GetAnnouncementPath(announcementId);
            _audioSystem.PlayGlobal(announcement, filter, true, audioParams);
        }

        /// <summary>
        ///     Sends an announcement with a message
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter, string message, string sender = "Central Command", Color? colorOverride = null, AudioParams? audioParams = null)
        {
            string announcement = GetAnnouncementPath(announcementId);
            _audioSystem.PlayGlobal(announcement, filter, true, audioParams);
            _chatSystem.DispatchGlobalAnnouncement(message, sender, false, colorOverride: colorOverride);
        }
    }
}
