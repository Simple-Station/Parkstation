using Content.Shared.SimpleStation14.Skills.Prototypes;
using Content.Shared.SimpleStation14.Skills.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Skills.Components
{
    /// <summary>
    ///    This holds all the data on an entity's skills.
    ///    It should generally never be accessed directly, and instead accessed through <see cref="SharedSkillsSystem"/>.
    ///    No, not event just to get a single value. I worked hard on those functions ;~;.
    /// </summary>
    [RegisterComponent]
    [NetworkedComponent]
    [AutoGenerateComponentState]
    public sealed partial class SkillsComponent : Component
    {
        /// <summary>
        ///     This holds all the data on an entity's skills.
        ///     If you're using this- don't. Use <see cref="SharedSkillsSystem"/> to access it.
        /// </summary>
        [AutoNetworkedField]
        [ViewVariables(VVAccess.ReadOnly)]
        public Dictionary<string, int> Skills = new();

        /// <summary>
        ///     Skills this entity starts with values in.
        ///     Make sure not to double this up with <see cref="SpeciesPrototype.SkillBonuses"/>.
        [DataField("startingSkills", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<int, SkillPrototype>))]
        public Dictionary<string, int> StartingSkills = new();
    }
}
