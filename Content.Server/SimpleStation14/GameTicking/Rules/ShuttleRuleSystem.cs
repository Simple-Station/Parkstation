using Content.Server.GameTicking;
using Content.Server.RoundEnd;

namespace Content.Server.SimpleStation14.GameTicking.Rules
{
    public sealed class ShuttleRuleSystem : EntitySystem
    {
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        }

        // TODO: Get it to show at bottom of manifest
        private void OnRoundEnd(RoundEndTextAppendEvent ev)
        {
            switch (_roundEndSystem._autoCalled)
            {
                case true:
                    ev.AddLine("");
                    ev.AddLine("");
                    ev.AddLine(Loc.GetString("shift-round-end-autocall"));
                    break;
                case false:
                    ev.AddLine("");
                    ev.AddLine("");
                    ev.AddLine(Loc.GetString("shift-round-end-emergency"));
                    break;
                default:
                    ev.AddLine("");
                    ev.AddLine("");
                    ev.AddLine(Loc.GetString("shift-round-end-unknown"));
                    break;
            }
        }
    }
}
