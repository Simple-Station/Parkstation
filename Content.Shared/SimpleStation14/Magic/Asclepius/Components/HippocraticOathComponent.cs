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
            // Brute
            { "Blunt", 0.25f },
            { "Slash", 0.2f },
            { "Piercing", 0.15f },
            // Burn
            { "Heat", 0.225f },
            { "Cold", 0.2f },
            { "Shock", 0.1f },
            // Airloss
            { "Airloss", 0.25f },
            { "Bloodloss", 0.1f },
            // Genetic
            { "Genetic", 0.05f },
        };
    }
}
