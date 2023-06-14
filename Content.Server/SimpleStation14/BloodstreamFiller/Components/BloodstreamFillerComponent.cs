using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.BloodstreamFiller.Components;

[RegisterComponent]
public sealed class BloodstreamFillerComponent : Component
{
    /// <summary>
    ///     The duration of the fill DoAfter, in seconds.
    /// </summary>
    [DataField("fillTime"), ViewVariables(VVAccess.ReadWrite)]
    public float FillTime = 1.0f;

    /// <summary>
    ///     The multiplier for the DoAfter time when attempting to fill yourself.
    /// </summary>
    [DataField("selfFillMutli"), ViewVariables(VVAccess.ReadWrite)]
    public float SelfFillMutli = 3.5f;

    /// <summary>
    /// The name of the volume to refill.
    /// </summary>
    /// <remarks>
    /// Should match the <see cref="SolutionContainerComponent"/> name or otherwise.
    /// </remarks>
    [DataField("solution"), ViewVariables(VVAccess.ReadWrite)]
    public string Solution { get; } = "filler";

    /// <summary>
    ///     The amount of reagent that this bloodstream filler will fill with at most.
    /// </summary>
    [DataField("amount"), ViewVariables(VVAccess.ReadWrite)]
    public float Amount = 100.0f;

    /// <summary>
    ///     The regent allowed to be used by this filler.
    /// </summary>
    /// <remarks>
    ///     If null, any reagent will be allowed.
    /// </remarks>
    [DataField("reagent"), ViewVariables(VVAccess.ReadWrite)]
    public string? Reagent = null;

    /// <summary>
    ///     Will this filler only fill Silicons?
    /// </summary>
    /// <remarks>
    ///     Somewhat niche use case, but Bloodstreams are very inflexible.
    /// </remarks>
    [DataField("siliconOnly"), ViewVariables(VVAccess.ReadWrite)]
    public bool SiliconOnly = true;

    /// <summary>
    ///     Can this bloodfiller be used to overfill someone?
    /// </summary>
    /// <remarks>
    ///     If true, the bloodfiller will be able to fill someone already at max blood, causing damage and spilling blood.
    /// </remarks>
    [DataField("overfill"), ViewVariables(VVAccess.ReadWrite)]
    public bool Overfill = true;

    /// <summary>
    ///     The multiplier for the DoAfter time when attempting to overfill someone.
    /// </summary>
    [DataField("overfillMutli"), ViewVariables(VVAccess.ReadWrite)]
    public float OverfillMutli = 5.5f;

    /// <summary>
    ///     The amount of damage dealt when overfilling someone.
    /// </summary>
    /// <remarks>
    ///     This must be specified in YAML.
    /// </remarks>
    [DataField("overfillDamage"), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier OverfillDamage = default!;

    #region Player Feedback
    /// <summary>
    ///     The sound played when the filler is used.
    /// </summary>
    [DataField("useSound")]
    public SoundSpecifier UseSound = new SoundPathSpecifier("/Audio/Effects/bang.ogg");

    /// <summary>
    ///     The sound played when the filler refills.
    /// </summary>
    [DataField("refillSound")]
    public SoundSpecifier RefillSound = new SoundPathSpecifier("/Audio/Effects/buckle.ogg");

    /// <summary>
    ///     The sound played when overfilling someone.
    /// </summary>
    [DataField("overfillSound")]
    public SoundSpecifier OverfillSound = new SoundPathSpecifier("/Audio/Effects/demon_dies.ogg");

    /// <summary>
    ///     The popup text when the filler is used.
    /// </summary>
    [DataField("usePopup")]
    public string UsePopup = "bloodfiller-use-user";

    /// <summary>
    ///     The popup text when the filler is used on you.
    /// </summary>
    [DataField("usedOnPopup")]
    public string UsedOnPopup = "bloodfiller-use-target";

    /// <summary>
    ///     The popup text when the bloodfiller is empty.
    /// </summary>
    [DataField("emptyPopup")]
    public string EmptyPopup = "bloodfiller-use-empty";

    /// <summary>
    ///     The popup text when the bloodfiller can't be used on the target.
    /// </summary>
    /// <remarks>
    ///     Due to <see cref="SiliconOnly"/> or otherwise.
    /// </remarks>
    [DataField("targetInvalidPopup")]
    public string TargetInvalidPopup = "bloodfiller-use-invalid-target";

    /// <summary>
    ///     The popup text when the bloodfiller doesn't have the target's blood.
    /// </summary>
    [DataField("targetBloodInvalidPopup")]
    public string TargetBloodInvalidPopup = "bloodfiller-use-invalid-blood";

    /// <summary>
    ///     The popup text when the filler is already full.
    /// </summary>
    [DataField("refillFullPopup")]
    public string RefillFullPopup = "bloodfiller-refill-filler-full";

    /// <summary>
    ///     The popup text when the tank is empty.
    /// </summary>
    [DataField("refillTankEmptyPopup")]
    public string RefillTankEmptyPopup = "bloodfiller-refill-tank-empty";

    /// <summary>
    ///     The popup text when trying to refill the bloodfiller from a tank with the wrong reagent.
    /// </summary>
    [DataField("refillReagentInvalidPopup")]
    public string RefillReagentInvalidPopup = "bloodfiller-refill-reagent-invalid";

    /// <summary>
    ///     The popup text when either a tank or filler contains a dirty mixture.
    /// </summary>
    [DataField("dirtyPopup")]
    public string DirtyPopup = "bloodfiller-reagent-dirty";

    /// <summary>
    ///     The popup text when trying to overfill someone.
    /// </summary>
    [DataField("targetOverfillPopup")]
    public string TargetOverfillPopup = "bloodfiller-user-overfill";

    /// <summary>
    ///     The popup text when getting overfilled.
    /// </summary>
    [DataField("overfilledPopup")]
    public string OverfilledPopup = "bloodfiller-target-overfill";
    #endregion
}
