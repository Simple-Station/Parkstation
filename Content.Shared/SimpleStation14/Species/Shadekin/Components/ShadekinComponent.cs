using Content.Shared.SimpleStation14.Species.Shadekin.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Species.Shadekin.Components
{
    [RegisterComponent, NetworkedComponent()]
    public sealed class ShadekinComponent : Component
    {
        ShadekinPowerSystem _powerSystem = new();

        // Dirty
        [ViewVariables(VVAccess.ReadOnly)]
        public float Accumulator = 0f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float AccumulatorRate = 3f;


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
        [ViewVariables(VVAccess.ReadOnly)]
        public Vector3 TintColor = new(0.5f, 0f, 0.5f);

        [ViewVariables(VVAccess.ReadWrite)]
        public float TintIntensity = 0.65f;


        // Power level
        /// <summary>
        ///     Current amount of energy.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevel {
            get => _powerLevel;
            set => _powerLevel = Math.Clamp(value, PowerLevelMin, PowerLevelMax);
        }
        public float _powerLevel = 0f;

        /// <summary>
        ///     Don't let PowerLevel go above this value.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly float PowerLevelMax = (float) PowerThresholds[ShadekinPowerThreshold.Max];

        /// <summary>
        ///     Blackeyes if PowerLevel is below this value.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly float PowerLevelMin = (float) PowerThresholds[ShadekinPowerThreshold.Min];

        /// <summary>
        ///     How much energy is gained per second.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevelGain = 2f;

        /// <summary>
        ///     Power gain multiplier
        ///     Modified by chems and while in The Dark.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float PowerLevelGainMultiplier = 1f;

        /// <summary>
        ///     Whether to gain power or not.
        ///     Disabled by chems.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool PowerLevelGainEnabled = true;

        /// <summary>
        ///     Whether they are a blackeye.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Blackeye = false;


        public static readonly Dictionary<ShadekinPowerThreshold, float> PowerThresholds = new()
        {
            { ShadekinPowerThreshold.Max, 250.0f },
            { ShadekinPowerThreshold.Great, 200.0f },
            { ShadekinPowerThreshold.Good, 150.0f },
            { ShadekinPowerThreshold.Okay, 100.0f },
            { ShadekinPowerThreshold.Tired, 50.0f },
            { ShadekinPowerThreshold.Min, 0.0f },
        };
    }

    [Serializable, NetSerializable]
    public sealed class ShadekinComponentState : ComponentState
    {
        public float PowerLevel { get; init; }
        public float PowerLevelGain { get; init; }
        public float PowerLevelGainMultiplier { get; init; }
        public bool PowerLevelGainEnabled { get; init; }
        public bool Blackeye { get; init; }
    }

    [Serializable, NetSerializable]
    public enum ShadekinPowerThreshold : byte
    {
        Max = 1 << 4,
        Great = 1 << 3,
        Good = 1 << 2,
        Okay = 1 << 1,
        Tired = 1 << 0,
        Min = 0,
    }
}
