using System.Linq;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.SimpleStation14.Announcements.Systems;
using Content.Server.StationEvents.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class FalseAlarmRule : StationEventSystem<FalseAlarmRuleComponent>
{
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Started(EntityUid uid, FalseAlarmRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var allEv = _event.AllEvents().Select(p => p.Value).ToList();
        var picked = RobustRandom.Pick(allEv);

        if (picked.StartAnnouncement != null)
        {
            _announcer.SendAnnouncement(args.RuleId, Filter.Broadcast(), Loc.GetString(picked.StartAnnouncement), colorOverride: Color.Gold);
        }
    }
}
