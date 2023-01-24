using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Speech.RandomBark
{
    [RegisterComponent]
    public sealed class RandomBarkComponent : Component
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        // Should the message be sent to the chat log?
        [DataField("chatlog")]
        public bool Chatlog = false;

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
            "Rawrl",
            "Eeeeeee",
            "Barkums",
            "Awooooooooooooooooooo awoo awoooo",
            "Grrrrrrrrrrrrrrrrrr",
            "Rarrwrarrwr",
            "Goddamn I love gold fish crackers",
            "Bork bork boof boof bork bork boof boof boof bork",
            "Bark",
            "Boof",
            "Woofums",
            "Rawrl",
            "Eeeeeee",
            "Barkums",
        };
    }
}
