using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.SimpleStation14.Damage.Events;
using Content.Shared.SimpleStation14.Damage.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Damage.Systems;

public sealed class ReagentDamageModifierSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReagentDamageModifierComponent, ReagentDamageModifyEvent>(OnReagentDamage);
    }


    /// <summary>
    ///     Applies a modifier set from the component to the reagent damage
    /// </summary>
    private void OnReagentDamage(EntityUid uid, ReagentDamageModifierComponent component, ref ReagentDamageModifyEvent args)
    {
        if (_prototype.TryIndex<DamageModifierSetPrototype>(component.ModifierSet, out var modifier))
        {
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifier);
        }
    }


    /// <summary>
    ///     Sends an event requesting reagent damage modifications since the HealthChange reagent effect can't send events
    /// </summary>
    /// <param name="uid">Entity to raise the event on</param>
    /// <param name="originalDamage">Damage to be taken for modification</param>
    /// <returns>Modified damage set</returns>
    public DamageSpecifier ModifyDamage(EntityUid uid, DamageSpecifier originalDamage)
    {
        var ev = new ReagentDamageModifyEvent(originalDamage);
        RaiseLocalEvent(uid, ref ev);
        return ev.Damage;
    }
}
