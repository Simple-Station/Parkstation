namespace Content.Shared.SimpleStation14.Nanites
{
    [RegisterComponent]
    public sealed class NaniteHostComponent : Component
    {
        [DataField("regenaccumulator")]
        public float regenAccumulator = 0f;
        [DataField("buttonaccumulator")]
        public float buttonAccumulator = 0f;
        [DataField("syncaccumulator")]
        public float syncAccumulator = 0f;


        /// <summary>
        /// Cloud sync ID, for every 10 seconds
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double CloudID = 0;

        /// <summary>
        /// List of currently installed programs
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public List<NaniteProgram> Programs = new();


        /// <summary>
        /// Amount of nanites active in the bloodstream
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double Nanites = 0;

        /// <summary>
        /// Default amount of nanites to have on startup (injection)
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double DefaultNanites = 50;

        /// <summary>
        /// Amount of nanites to preserve, stopping programs from consuming them
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double SafetyLevel = 50;


        /// <summary>
        /// Amount of nanites to preserve, stopping programs from consuming them
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public double DefaultMax = 500;

        /// <summary>
        /// Amount of nanites to preserve, stopping programs from consuming them
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double Max = 500;


        /// <summary>
        /// How fast the nanites regenerate, every second
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public double DefaultRegenSpeed = 0.5;

        /// <summary>
        /// How fast the nanites regenerate, every second
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public double RegenSpeed = 0.5;
    }
}
