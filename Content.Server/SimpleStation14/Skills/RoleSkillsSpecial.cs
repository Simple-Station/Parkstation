using Content.Shared.Roles;
using Content.Shared.SimpleStation14.Skills.Prototypes;
using Content.Shared.SimpleStation14.Skills.Systems;
using JetBrains.Annotations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server.SimpleStation14.Skills;

/// <summary>
///     Adds to an Entity's skills on equip.
/// </summary>
[UsedImplicitly]
public sealed class RoleSkillsSpecial : JobSpecial
{

    [DataField("skills", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<int, SkillPrototype>))]
    public Dictionary<string, int> Skills { get; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var skillSystem = entMan.System<SharedSkillsSystem>();

        foreach (var skill in Skills)
            skillSystem.TryModifySkillLevel(mob, skill.Key, skill.Value);
    }
}
