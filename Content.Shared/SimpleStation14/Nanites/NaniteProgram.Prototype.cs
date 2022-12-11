using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Nanites
{
    [Prototype("naniteProgram")]
    public sealed class NaniteProgramPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataFieldAttribute]
        public string ID { get; } = default!;


        /// <summary>
        /// Which program is this in the code?
        /// </summary>
        [DataField("program", required: true), ViewVariables]
        public string Type = "Program";

        /// <summary>
        /// Should this show in the program hub if it is unlocked?
        /// </summary>
        [DataField("showInHub", required: true), ViewVariables]
        public bool ShowInHub = true;
    }
}
