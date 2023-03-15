namespace Content.Shared.SimpleStation14.Magic.Asclepius.Components
{
    [RegisterComponent]
    public class HippocraticOathComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public float HealingAccumulator = 0f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float HealingTime = 1f;

        /// <summary>
        ///     Damage types as strings, and the amount to heal.
        ///     Damage gets inverted in code, do not put a negative number here!
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public Dictionary<string, float> DamageTypes = new()
        {
            // Overall 1.075
            // Brute (0.45)
            { "Blunt", 0.20f },
            { "Slash", 0.125f },
            { "Piercing", 0.125f },
            // Burn (0.4)
            { "Heat", 0.16f },
            { "Cold", 0.16f },
            { "Shock", 0.08f },
            // Airloss (0.2)
            { "Airloss", 0.15f },
            { "Bloodloss", 0.05f },
            // Genetic (0.025)
            { "Genetic", 0.025f },
        };
    }
}
