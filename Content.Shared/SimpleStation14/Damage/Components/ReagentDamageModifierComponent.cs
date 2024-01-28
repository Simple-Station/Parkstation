using Content.Shared.Damage.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.SimpleStation14.Damage.Components;

[RegisterComponent]
public sealed class ReagentDamageModifierComponent : Component
{
    /// <summary>
    ///     Modifier set prototype to use for reagent damages
    /// </summary>
    [DataField("modifierSet", customTypeSerializer: typeof(PrototypeIdSerializer<DamageModifierSetPrototype>), required: true)]
    public string ModifierSet = "";
}
