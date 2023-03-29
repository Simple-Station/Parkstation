using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusHealingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var component in _entityManager.EntityQuery<AsclepiusStaffComponent>()
            .Where(x =>
                x.BoundTo != EntityUid.Invalid &&
                _entityManager.TryGetComponent<HippocraticOathComponent>(x.BoundTo, out var _)
            ))
            {
                foreach (var entity in _lookupSystem.GetEntitiesInRange(component.Owner, 7f, LookupFlags.Uncontained)
                .Where(y =>
                    _entityManager.TryGetComponent<DamageableComponent>(y, out var damageable) &&
                    damageable.TotalDamage > 0f
                ))
                {
                    var oath = _entityManager.GetComponent<HippocraticOathComponent>(component.BoundTo);

                    oath.HealingAccumulator += frameTime;
                    if (oath.HealingAccumulator < oath.HealingTime)
                    {
                        continue;
                    }
                    oath.HealingAccumulator = 0f;

                    foreach (var (type, amount) in oath.DamageTypes)
                    {
                        if (!_prototypeManager.TryIndex<DamageGroupPrototype>(type, out var group))
                        {
                            if (!_prototypeManager.TryIndex<DamageTypePrototype>(type, out var proto))
                            {
                                DebugTools.Assert($"[Asclepius Healing]: Damage type {type} does not exist!");
                                continue;
                            }

                            _damageableSystem.TryChangeDamage(component.BoundTo, new DamageSpecifier(proto, -amount));
                        }
                        else
                        {
                            foreach (var damageType in group.DamageTypes)
                            {
                                if (!_prototypeManager.TryIndex<DamageTypePrototype>(damageType, out var proto))
                                {
                                    DebugTools.Assert($"[Asclepius Healing]: Damage type {damageType} does not exist!");
                                    continue;
                                }

                                _damageableSystem.TryChangeDamage(component.BoundTo, new DamageSpecifier(proto, -(amount / group.DamageTypes.Count)));
                            }
                        }
                    }
                }
            }
        }
    }
}
