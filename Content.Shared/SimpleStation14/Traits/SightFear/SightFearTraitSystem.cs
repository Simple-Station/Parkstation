using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Interaction.Helpers;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.SimpleStation14.Traits.SightFear;

public sealed class SightFearTraitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SightFearTraitComponent, ComponentInit>(OnComponentInit);
    }


    private void OnComponentInit(EntityUid uid, SightFearTraitComponent component, ComponentInit args)
    {
        if (!string.IsNullOrEmpty(component.AfraidOf) ||
            !_prototype.TryIndex<WeightedRandomPrototype>("RandomFears", out var randomFears))
            return;

        component.AfraidOf = randomFears.Pick(_random);
        Dirty(component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SightFearTraitComponent>();
        while(query.MoveNext(out var uid, out var component))
        {
            var range = 10f;
            if (_entity.TryGetComponent<SharedEyeComponent>(uid, out var eye))
                range *= (eye.Zoom.X + eye.Zoom.Y) / 2;

            var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range)
                .Where(e => _entity.HasComponent<SightFearedComponent>(e));

            foreach (var entity in entities)
            {
                if (!_entity.TryGetComponent<ExaminerComponent>(uid, out var examiner) ||
                    !examiner.InRangeUnOccluded(Transform(entity).Coordinates, range))
                    continue;

                var feared = _entity.GetComponent<SightFearedComponent>(entity);
                if (!feared.Fears.TryGetValue(component.AfraidOf, out var value))
                    continue;

                var distance = (Transform(uid).Coordinates.Position - Transform(entity).Coordinates.Position).Length;
                var strength = MathHelper.Lerp(0f, value, 1f - (distance / range));
                if (strength <= 0f)
                    continue;

                Logger.ErrorS("SightFearTraitSystem", $"Entity {uid} is afraid of {entity} ({component.AfraidOf}) at strength {strength}");
            }
        }
    }
}
