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

        public override void Initialize()
        {
            base.Initialize();

            PickAnnouncer();

            _configManager.OnValueChanged(SimpleStationCCVars.Announcer, SetAnnouncer);

            SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        }


        private void OnRoundStarting(RoundStartingEvent ev)
        {
            SetAnnouncer(_configManager.GetCVar<string>(SimpleStationCCVars.Announcer));
        }
    }
}
