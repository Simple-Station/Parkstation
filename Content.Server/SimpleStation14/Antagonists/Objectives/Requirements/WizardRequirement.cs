using Content.Server.Objectives.Interfaces;
using Content.Server.SimpleStation14.Wizard;
using JetBrains.Annotations;

namespace Content.Server.Objectives.Requirements
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class WizardRequirement : IObjectiveRequirement
    {
        public bool CanBeAssigned(Mind.Mind mind)
        {
            return mind.HasRole<WizardRole>();
        }
    }
}
