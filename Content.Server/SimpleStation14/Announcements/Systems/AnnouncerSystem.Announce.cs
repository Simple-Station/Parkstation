using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.SimpleStation14.Announcements.Prototypes;
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
        public string GetAnnouncementPath(string announcerId, string announcementId)
        {
            var announcer = _prototypeManager.EnumeratePrototypes<AnnouncerPrototype>().ToArray().First(a => a.ID == announcerId);

            var announcementType = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
                Announcer.AnnouncementPaths.First(a => a.ID == "fallback");

            if (announcementType.Path != null)
                return $"{announcer.BasePath}/{announcementType.Path}";
            else if (announcementType.Collection != null)
                return $"{new SoundCollectionSpecifier(announcementType.Collection).GetSound()}";
            return $"{announcer.BasePath}/{announcementType.Path}";
        }

        public string? GetAnnouncementMessage(string announcerId, string announcementId)
        {
            string? result = null;

            var announcer = _prototypeManager.EnumeratePrototypes<AnnouncerPrototype>().ToArray().First(a => a.ID == announcerId);

            var announcementType = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
                Announcer.AnnouncementPaths.First(a => a.ID == "fallback");

            if (announcementType.MessageOverride != null)
                result = Loc.GetString(announcementType.MessageOverride);

            return result;
        }


        /// <summary>
        ///     Sends an announcement
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter, AudioParams? audioParams = null)
        {
            var announcement = GetAnnouncementPath(Announcer.ID, announcementId);
            _audioSystem.PlayGlobal(announcement, filter, true, audioParams);
        }

        /// <summary>
        ///     Sends an announcement with a message
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter, string message, string? sender = null, Color? colorOverride = null, AudioParams? audioParams = null)
        {
            var announcementPath = GetAnnouncementPath(Announcer.ID, announcementId);
            sender ??= Announcer.Name;

            var announcementMessage = GetAnnouncementMessage(Announcer.ID, announcementId);
            if (announcementMessage != null)
                message = announcementMessage;

            _audioSystem.PlayGlobal(announcementPath, filter, true, audioParams);
            _chatSystem.DispatchGlobalAnnouncement(message, sender, false, colorOverride: colorOverride);
        }
    }
}
