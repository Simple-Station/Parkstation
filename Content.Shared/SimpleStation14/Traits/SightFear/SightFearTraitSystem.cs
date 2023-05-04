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
        SubscribeLocalEvent<SightFearedComponent, ComponentInit>(CheckFeared);
    }


    private void OnComponentInit(EntityUid uid, SightFearTraitComponent component, ComponentInit args)
    {
        if (!string.IsNullOrEmpty(component.AfraidOf) ||
            !_prototype.TryIndex<WeightedRandomPrototype>("RandomFears", out var randomFears))
            return;

        component.AfraidOf = randomFears.Pick(_random);
        Dirty(component);
    }

    private void CheckFeared(EntityUid uid, SightFearedComponent component, ComponentInit args)
    {
        if (component.Fears.Count == 0)
        {
            Logger.WarningS("SightFearTraitSystem", $"Entity {uid} has SightFearedComponent without any defined fears.");
            return;
        }

        if (!_prototype.TryIndex<WeightedRandomPrototype>("RandomFears", out var randomFears))
        {
            Logger.ErrorS("SightFearTraitSystem", $"Prototype RandomFears could not be found.");
            return;
        }

        foreach (var fear in component.Fears.Keys.Where(fear => !randomFears.Weights.ContainsKey(fear)))
        {
            Logger.ErrorS("SightFearTraitSystem", $"Prototype RandomFears does not contain fear {fear} from SightFearedComponent on entity {uid}.");
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SightFearTraitComponent>();
        while(query.MoveNext(out var uid, out var component))
        {
            if (component.Accumulator > 0f)
            {
                component.Accumulator -= frameTime;
                continue;
            }

            component.Accumulator = 0.84f;

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
