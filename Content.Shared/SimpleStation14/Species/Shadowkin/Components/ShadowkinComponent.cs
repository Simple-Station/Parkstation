using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Components
{
    [RegisterComponent, NetworkedComponent()]
    public sealed class ShadowkinComponent : Component
    {
        // Dirty
        [ViewVariables(VVAccess.ReadOnly)]
        public float DirtyAccumulator = 0f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float DirtyAccumulatorRate = 3f;


        // Random occurrences
        [ViewVariables(VVAccess.ReadOnly)]
        public float ForceSwapAccumulator = 0f;

        /// <summary>
        ///     Default value.
        ///     Gets randomly set after activated.
        ///     Forces DarkSwap ability.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float ForceSwapRate;

        [ViewVariables(VVAccess.ReadWrite)]
        public float ForceSwapRateMin = 30f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float ForceSwapRateMax = 90f;

        [ViewVariables(VVAccess.ReadOnly)]
        public float TiredAccumulator = 0f;

        /// <summary>
        ///     Default value.
        ///     Forces the rest ability.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float TiredRate;

        [ViewVariables(VVAccess.ReadWrite)]
        public float TiredRateMin = 15f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float TiredRateMax = 50f;


        // Darkening
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Darken = false;

        [ViewVariables(VVAccess.ReadWrite)]
        public float DarkenRange = 5f;

        [ViewVariables(VVAccess.ReadOnly)]
        public List<EntityUid> DarkenedLights = new();

        // Accumulator for darkening
        [ViewVariables(VVAccess.ReadWrite)]
        public float DarkenRate = 0.084f; // 1/12th of a second

        [ViewVariables(VVAccess.ReadOnly)]
        public float DarkenAccumulator = 0f;


        // Shader
        /// <summary>
        ///     Automatically set to eye color.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public Vector3 TintColor = new(0.5f, 0f, 0.5f);

        /// <summary>
        ///     Based on PowerLevel.
        /// </summary>
        /// <remarks>
        ///     *Will be based on PowerLevel.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        public float TintIntensity = 0.65f;


        // Power level
        /// <summary>
        ///     Current amount of energy.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevel
        {
            get => _powerLevel;
            set => _powerLevel = Math.Clamp(value, PowerLevelMin, PowerLevelMax);
        }
        public float _powerLevel;

        /// <summary>
        ///     Don't let PowerLevel go above this value.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly float PowerLevelMax = PowerThresholds[ShadowkinPowerThreshold.Max];

        /// <summary>
        ///     Blackeyes if PowerLevel is this value.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly float PowerLevelMin = PowerThresholds[ShadowkinPowerThreshold.Min];

        /// <summary>
        ///     How much energy is gained per second.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevelGain = 2f;

        /// <summary>
        ///     Power gain multiplier
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevelGainMultiplier = 1f;

        /// <summary>
        ///     Whether to gain power or not.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool PowerLevelGainEnabled = true;

        /// <summary>
        ///     Whether they are a blackeye.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Blackeye = false;


        public static readonly Dictionary<ShadowkinPowerThreshold, float> PowerThresholds = new()
        {
            { ShadowkinPowerThreshold.Max, 250.0f },
            { ShadowkinPowerThreshold.Great, 200.0f },
            { ShadowkinPowerThreshold.Good, 150.0f },
            { ShadowkinPowerThreshold.Okay, 100.0f },
            { ShadowkinPowerThreshold.Tired, 50.0f },
            { ShadowkinPowerThreshold.Min, 0.0f },
        };
    }

    [Serializable, NetSerializable]
    public sealed class ShadowkinComponentState : ComponentState
    {
        public float PowerLevel { get; init; }
        public float PowerLevelGain { get; init; }
        public float PowerLevelGainMultiplier { get; init; }
        public bool PowerLevelGainEnabled { get; init; }
        public bool Blackeye { get; init; }
    }

    [Serializable, NetSerializable]
    public enum ShadowkinPowerThreshold : byte
    {
        Max = 1 << 4,
        Great = 1 << 3,
        Good = 1 << 2,
        Okay = 1 << 1,
        Tired = 1 << 0,
        Min = 0,
    }
}
