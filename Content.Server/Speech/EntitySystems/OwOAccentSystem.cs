using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class OwOAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly IReadOnlyList<string> Prefixes = new List<string>{
            "<3", "HIII!", "Haiiii,", "H-hewwo?", "(#o.o)", ";;w;;", ";w;"
        }.AsReadOnly();

        private static readonly IReadOnlyList<string> Faces = new List<string>{
            "(·`ω´·)", ";;w;;", ";w;", "OwO", "UwU", ">w<", "^w^", "TwT", "-w-", "(^U^)", "✪ω✪", "(^▽^)", "(^///^)", "x3"
        }.AsReadOnly();

        private static readonly IReadOnlyList<string> CFaces = new List<string>{
            ">_<", "(^///^)", "(._. )", "(⊙_⊙#)", "x3", ":3", ";3", ";-;"
        }.AsReadOnly();

        private static readonly IReadOnlyList<string> Suffixes = new List<string>{
            "Ɛ>", "baii!", "bye bye!", "ceeya! :D"
        }.AsReadOnly();

        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            // TODO: Case insensitive..
            { "YOU", "WU" },
            { "You", "Wu" },
            { "you", "wu" },

            { "FUCK", "WUH OH" },
            { "Fuck", "Wuh oh" },
            { "fuck", "wuh oh"},

            { "NO", "NU" },
            { "No", "Nu" },
            { "no", "nu" },

            { "HAS", "HAZ" },
            { "Has", "Haz" },
            { "has", "haz" },

            { "SAYS", "SEZ" },
            { "Says", "Sez" },
            { "says", "sez" },

            { "THE", "DA" },
            { "The", "Da" },
            { "the", "da" },
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<OwOAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            // Pretty much ReplacementAccent
            foreach (var (word, repl) in SpecialWords) message = message.Replace(word, repl);

            // Random prefix
            if (_random.Next(1, 12) == 5 && message.Length > 12)
                message = $"{_random.Pick(Prefixes)} {message}";

            // Exclaim
            if (message != "!")
                message = message.Replace("! ", $"! {_random.Pick(Faces)} ");
            if (message.EndsWith("!") && message != "!")
                message = message.Substring(0, message.Length - 1) + message.Substring(message.Length - 1).Replace("!", $"! {_random.Pick(Faces)}");

            // Question
            if (message != "?") message = message.Replace("? ", $"? {_random.Pick(CFaces)} ");
            if (message.EndsWith("?") && message != "?")
                message = message.Substring(0, message.Length - 1) + message.Substring(message.Length - 1).Replace("?", $"! {_random.Pick(CFaces)}");

            // Random suffix, not affected by ! and ? reformatting and won't do if theres punctuation
            if (!message.EndsWith("!") && !message.EndsWith("?")
                && !message.EndsWith(".") && !message.EndsWith(",")
                && _random.Next(1, 10) == 5 && message.Length > 12)
                    message = $"{message}, {_random.Pick(Suffixes)}";

            // Slur letters
            message = message
                .Replace("r", "w").Replace("R", "W")
                .Replace("l", "w").Replace("L", "W");

            // "You're going to need a great lawyer for this! .. You do have a good lawyer, right?"
            // "Wu're going to need a gweat wawyew fow this! UwU .. wu do have a good wawyew, wight? (._. )"
            return message;
        }

        private void OnAccent(EntityUid uid, OwOAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
