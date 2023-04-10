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

        /// <summary>
        ///     Gets an announcement message from the announcer
        /// </summary>
        public string? GetAnnouncementMessage(string announcerId, string announcementId)
        {
            string? result = null;

            var announcer = _prototypeManager.EnumeratePrototypes<AnnouncerPrototype>().ToArray().First(a => a.ID == announcerId);

            var announcementType = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID.ToLower() == announcementId.ToLower()) ??
                Announcer.AnnouncementPaths.First(a => a.ID.ToLower() == "fallback");

            if (announcementType.MessageOverride != null)
                result = Loc.GetString(announcementType.MessageOverride);

            return result;
        }

        /// <summary>
        ///     Gets audio params from the announcer
        /// </summary>
        public AudioParams? GetAudioParams(string announcerId, string announcementId)
        {
            var announcer = _prototypeManager.EnumeratePrototypes<AnnouncerPrototype>().ToArray().First(a => a.ID == announcerId);

            var announcementType = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
                Announcer.AnnouncementPaths.First(a => a.ID == "fallback");

            return announcementType.AudioParams;
        }


        /// <summary>
        ///     Sends an announcement audio
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter)
        {
            var announcement = GetAnnouncementPath(Announcer.ID, announcementId.ToLower());
            _audioSystem.PlayGlobal(announcement, filter, true, GetAudioParams(Announcer.ID, announcementId.ToLower()));
        }

        /// <summary>
        ///     Sends an announcement message
        /// </summary>
        public void SendAnnouncement(string announcementId, string message, string? sender = null, Color? colorOverride = null, EntityUid? station = null)
        {
            sender ??= Announcer.Name;

            var announcementMessage = GetAnnouncementMessage(Announcer.ID, announcementId.ToLower());
            if (announcementMessage != null)
                message = announcementMessage;

            if (station == null) {
                _chatSystem.DispatchGlobalAnnouncement(message, sender, false, colorOverride: colorOverride);
            } else {
                _chatSystem.DispatchStationAnnouncement(station.Value, message, sender, false, colorOverride: colorOverride);
            }
        }

        /// <summary>
        ///     Sends an announcement with a message and audio
        /// </summary>
        public void SendAnnouncement(string announcementId, Filter filter, string message, string? sender = null, Color? colorOverride = null, EntityUid? station = null)
        {
            var announcementPath = GetAnnouncementPath(Announcer.ID, announcementId.ToLower());
            sender ??= Announcer.Name;

            var announcementMessage = GetAnnouncementMessage(Announcer.ID, announcementId.ToLower());
            if (announcementMessage != null)
                message = announcementMessage;

            _audioSystem.PlayGlobal(announcementPath, filter, true, GetAudioParams(Announcer.ID, announcementId.ToLower()));
            if (station == null)
            {
                _chatSystem.DispatchGlobalAnnouncement(message, sender, false, colorOverride: colorOverride);
            }
            else
            {
                _chatSystem.DispatchStationAnnouncement(station.Value, message, sender, false, colorOverride: colorOverride);
            }
        }
    }
}
