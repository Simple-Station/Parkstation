using System.Linq;
using Content.Server.SimpleStation14.Announcements.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Announcements.Systems
{
    public sealed partial class AnnouncerSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        /// <summary>
        ///     Refreshes the announcers cache
        /// </summary>
        public void RefreshAnnouncers()
        {
            Announcers = _prototypeManager.EnumeratePrototypes<AnnouncerPrototype>().ToArray();
        }

        /// <summary>
        ///     Picks a random announcer
        /// </summary>
        public void PickAnnouncer()
        {
            Announcer = _random.Pick(Announcers);
        }

        /// <summary>
        ///     Sets the announcer
        /// </summary>
        public void PickAnnouncer(string announcerId)
        {
            if (announcerId == "random")
            {
                PickAnnouncer();
                return;
            }

            Announcer = _prototypeManager.Index<AnnouncerPrototype>(announcerId);
        }
    }
}
