using Content.Shared.Damage;

namespace Content.Shared.SimpleStation14.Damage.Events;

[ByRefEvent]
public sealed class ReagentDamageModifyEvent : EntityEventArgs
{
    public readonly DamageSpecifier OriginalDamage;
    public DamageSpecifier Damage;

    public ReagentDamageModifyEvent(DamageSpecifier damage)
    {
        OriginalDamage = damage;
        Damage = damage;
    }
}
