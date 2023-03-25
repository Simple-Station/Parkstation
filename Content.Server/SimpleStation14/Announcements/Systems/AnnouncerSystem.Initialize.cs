using Content.Server.GameTicking.Events;
using Content.Server.SimpleStation14.Announcements.Prototypes;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.SimpleStation14.Announcements.Systems
{
    public sealed partial class AnnouncerSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;

        /// <summary>
        ///     The currently selected announcer
        /// </summary>
        public AnnouncerPrototype Announcer { get; set; } = default!;
        /// <summary>
        ///     A cached list of all announcers
        /// </summary>
        public AnnouncerPrototype[] Announcers { get; set; } = default!;

        public override void Initialize()
        {
            base.Initialize();

            RefreshAnnouncers();
            PickAnnouncer();

            _configManager.OnValueChanged(SimpleStationCVars.Announcer, PickAnnouncer);

            SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        }


        private void OnRoundStarting(RoundStartingEvent ev)
        {
            RefreshAnnouncers();
            PickAnnouncer(_configManager.GetCVar<string>(SimpleStationCVars.Announcer));
        }
    }
}
