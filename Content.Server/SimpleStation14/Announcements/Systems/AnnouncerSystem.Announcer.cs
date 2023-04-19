using System.Linq;
using Content.Server.SimpleStation14.Announcements.Prototypes;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Announcements.Systems
{
    public sealed partial class AnnouncerSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        /// <summary>
        ///     Picks a random announcer
        /// </summary>
        public void PickAnnouncer()
        {
            Announcer = _random.Pick(_prototypeManager.EnumeratePrototypes<AnnouncerPrototype>()
                .Where(x => !_configManager.GetCVar(SimpleStationCCVars.AnnouncerBlacklist).Contains(x.ID))
                .ToArray());
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
