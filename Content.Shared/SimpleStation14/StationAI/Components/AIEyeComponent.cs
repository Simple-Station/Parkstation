namespace Content.Shared.SimpleStation14.StationAI
{
    [RegisterComponent]
    public sealed class AIEyeComponent : Component
    {
        /// <summary>
        ///     The grace period the eye gets once a new camera is found before it will switch to it, to avoid flickering.
        /// </summary>
        [DataField("gracePeriod"), ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan GracePeriod = TimeSpan.FromSeconds(0.65);

        /// <summary>
        ///     The time at which the eye will switch to a new camera, assuming <see cref="GracePeriod"/> is used.
        /// </summary>
        public TimeSpan SwitchTime;

        /// <summary>
        ///     Whether the grace period is currently ticking.
        /// </summary>
        public bool GracePeriodTicking = false;
    }
}
