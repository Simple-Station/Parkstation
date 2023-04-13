using System.Linq;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Announcements.Systems
{
    public sealed partial class AnnouncerSystem
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
        /// <param name="announcerId">ID of the announcer to choose</param>
        public void SetAnnouncer(string announcerId)
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
