namespace Content.Shared.SimpleStation14.Nanites
{
    public sealed class NaniteProgram
    {
        /// <summary>
        /// Does it do?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool EEnabled = true;


        /// <summary>
        /// Name (ID) of the program
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string EName = "Program";

        /// <summary>
        /// Description shown on the editor
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string EDescription = "Description";

        /// <summary>
        /// Program unique ID
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int Euid = 0;


        /// <summary>
        /// Trigger ID
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int ETrigger = 0;

        /// <summary>
        /// Enable ID
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int EEnable = 0;

        /// <summary>
        /// Disable ID
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int EDisable = 0;


        /// <summary>
        /// Identifier used for figuring out a type of program since Name is E(ditable)
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string Type = "Program";

        /// <summary>
        /// Event to trigger when triggered
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string OnTriggerEvent = "NaniteTrigger";

        /// <summary>
        /// Event to activate when the program is being deleted
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string OnDeleteEvent = "NaniteProgramDeleted";
    }
}
