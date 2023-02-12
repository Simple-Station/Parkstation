using System.Text.RegularExpressions;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Speech.EntitySystems
{
    public sealed class ShadekinAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly Regex mRegex = new(@"[adgjmps]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex aRegex = new(@"[behknqtwy]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rRegex = new(@"[cfiloruxz]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void Initialize()
        {
            SubscribeLocalEvent<ShadekinAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            message = message.Trim();

            // Replace letters with other letters
            message = mRegex.Replace(message, "m");
            message = aRegex.Replace(message, "a");
            message = rRegex.Replace(message, "r");

            return message;
        }

        private void OnAccent(EntityUid uid, ShadekinAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
