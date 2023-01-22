using Content.Server.GameTicking;
using Content.Server.RoundEnd;

namespace Content.Server.SimpleStation14.GameTicking
{
    public sealed class ShuttleRuleSystem : EntitySystem
    {
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        }

        private void OnRoundEnd(RoundEndTextAppendEvent ev)
        {
            // TODO: Get it to show at bottom of manifest
            if (_roundEndSystem._autoCalled == true)
            {
                ev.AddLine("");
                ev.AddLine("");
                ev.AddLine(Loc.GetString("shift-round-end-autocall"));
            }
            else if (_roundEndSystem._autoCalled == false)
            {
                ev.AddLine("");
                ev.AddLine("");
                ev.AddLine(Loc.GetString("shift-round-end-emergency"));
            }
            else
            {
                ev.AddLine("");
                ev.AddLine("");
                ev.AddLine(Loc.GetString("shift-round-end-unknown"));
            }
        }
    }
}
