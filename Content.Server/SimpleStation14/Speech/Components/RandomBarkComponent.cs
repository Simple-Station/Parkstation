using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Speech.RandomBark
{
    [RegisterComponent]
    public sealed class RandomBarkComponent : Component
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        // Minimum time an animal will go without speaking
        [DataField("minTime")]
        public int MinTime = 45;

        // Maximum time an animal will go without speaking
        [DataField("maxTime")]
        public int MaxTime = 350;

        // Counter
        [DataField("barkAccumulator")]
        public float BarkAccumulator = 8f;

        // Multiplier applied to the random time. Good for changing the frequency without having to specify exact values
        [DataField("barkMultiplier")]
        public float BarkMultiplier = 1f;

        // List of things to be said. Filled with garbage to be modified by an accent, but can be specified in the .yml
        [DataField("barks"), ViewVariables(VVAccess.ReadWrite)]
        public IReadOnlyList<string> Barks = new[] {
            "Bark",
            "Boof",
            "Woofums",
            "Howling",
            "Screeeeeeeeeeee",
            "Barkums",
            "OH MY GOD THE FIRE IT BURNS IT BURNS SO BAD!!",
            "Death was here ;)",
            "Ha ha not really lol",
            "Goddamn I love gold fish crackers",
            "This is going to be the really long default text, so I'm just going to keep typing and rambling on. You ever tried a churro? They're actually really good. I wasn't expecting much from them, thought they'd be like, dry and crunchy, but no, they're like a cakey thing. Yeah, right, you'd expect something drier, but no, it's actually soft and tasty, sugary, they're really nice."
        };
    }
}
