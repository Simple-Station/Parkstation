using Content.Shared.SimpleStation14.Skills.Systems; // Parkstation-Skills
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Tools.Components
{
    [RegisterComponent, NetworkedComponent] // TODO move tool system to shared, and make it a friend.
    public sealed class ToolComponent : Component
    {
        [DataField("qualities")]
        public PrototypeFlags<ToolQualityPrototype> Qualities { get; set; } = new();

        /// <summary>
        ///     For tool interactions that have a delay before action this will modify the rate, time to wait is divided by this value
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("speed")]
        public float SpeedModifier { get; set; } = 1;

        [DataField("useSound")]
        public SoundSpecifier? UseSound { get; set; }

        // Parkstation-Skills-Start
        /// <summary>
        ///     The <see cref="SkillUseData"/> for the usage of this tool.
        /// </summary>
        [DataField("skillUsed")]
        public string? SkillUsed { get; set; }

        /// <summary>
        ///     The 'expected' skill level for this tool. Below this level lowers the use speed, above it raises it.
        /// </summary>
        /// <remarks>
        ///     Unused if <see cref="SkillUsed"/> is null.
        /// </remarks>
        [DataField("skillExpected")]
        public float SkillExpected { get; set; } = 3;
        // Parkstation-Skills-End
    }

    /// <summary>
    ///     Attempt event called *before* any do afters to see if the tool usage should succeed or not.
    /// </summary>
    public sealed class ToolUseAttemptEvent : CancellableEntityEventArgs
    {
        public EntityUid User { get; }

        public ToolUseAttemptEvent(EntityUid user)
        {
            User = user;
        }
    }

    /// <summary>
    /// Event raised on the user of a tool to see if they can actually use it.
    /// </summary>
    [ByRefEvent]
    public struct ToolUserAttemptUseEvent
    {
        public EntityUid? Target;
        public bool Cancelled = false;

        public ToolUserAttemptUseEvent(EntityUid? target)
        {
            Target = target;
        }
    }
}
