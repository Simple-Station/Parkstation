using Content.Server.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOnLandSystem : EntitySystem
    {
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DamageOnLandComponent, LandEvent>(DamageOnLand);
        }
        // PARK Allow for dealing damage on a roll, defaults to 100%
        private void DamageOnLand(EntityUid uid, DamageOnLandComponent component, ref LandEvent args)
        {
            if (_random.NextFloat(0, 100) > component.Chance) return;
            _damageableSystem.TryChangeDamage(uid, component.Damage, component.IgnoreResistances);
        }
    }
}
