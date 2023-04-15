using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

// Parkstation sleeping mods
using Robust.Shared.Audio;

namespace Content.Shared.Bed.Sleep;

/// <summary>
/// Added to entities when they go to sleep.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed class SleepingComponent : Component
{
    /// <summary>
    /// How much damage of any type it takes to wake this entity.
    /// </summary>
    [DataField("wakeThreshold")]
    public FixedPoint2 WakeThreshold = FixedPoint2.New(2);

    /// <summary>
    ///     Cooldown time between users hand interaction.
    /// </summary>
    [DataField("cooldown")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(1f);

    [DataField("cooldownEnd", customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan CoolDownEnd;

    // Parkstation sleeping mods
    [DataField("snore"), ViewVariables(VVAccess.ReadWrite)]
    public bool Snore = true;

    [DataField("snoreSounds")]
    public SoundCollectionSpecifier SnoreSounds = new SoundCollectionSpecifier("Snores", AudioParams.Default.WithVariation(0.2f));

    [DataField("heal"), ViewVariables(VVAccess.ReadWrite)]
    public bool Heal = true;
    // \Parkstation
}
