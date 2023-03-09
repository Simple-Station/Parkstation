namespace Content.Shared.SimpleStation14.Traits
{
    /// <summary>
    ///     Speeds up movement.
    ///     Speeds up climbing.
    /// </summary>
    [RegisterComponent]
    public sealed class LightWeightTraitComponent : Component
    {
        /// <summary>
        /// How much time to remove from climbing (in seconds)
        /// </summary>
        [DataField("negateTime"), ViewVariables(VVAccess.ReadWrite)]
        public float? NegateTime;


        // Actual speed names (after park change)
        /// <summary>
        /// MovementSpeedModifierComponent.BaseWalkSpeed
        /// </summary>
        [DataField("sprint"), ViewVariables(VVAccess.ReadOnly)]
        public float Sprint = 1f;

        /// <summary>
        /// MovementSpeedModifierComponent.BaseSprintSpeed
        /// </summary>
        [DataField("walk"), ViewVariables(VVAccess.ReadOnly)]
        public float Walk = 1.25f;
    }
}
