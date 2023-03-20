using Content.Shared.Actions.ActionTypes;

namespace Content.Shared.SimpleStation14.StationAI
{
    [RegisterComponent]
    public sealed class AIEyePowerComponent : Component
    {
        [DataField("prototype")]
        public string Prototype = "AIeye";

        public InstantAction? EyePowerAction = null;
    }
}
