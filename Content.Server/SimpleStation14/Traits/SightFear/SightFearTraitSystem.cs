using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Interaction.Helpers;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Traits.SightFear;

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
        // If the entity already has a fear, don't overwrite it
        // Check for RandomFears prototype
        if (!string.IsNullOrEmpty(component.AfraidOf) ||
            !_prototype.TryIndex<WeightedRandomPrototype>("RandomFears", out var randomFears))
            return;

        // Pick a random fear and use it
        component.AfraidOf = randomFears.Pick(_random);

        // Mark the component as dirty so it gets synced to the client
        Dirty(component);
    }

    private void CheckFeared(EntityUid uid, SightFearedComponent component, ComponentInit args)
    {
        // Check if the entity has any fears defined
        if (component.Fears.Count == 0)
        {
            Logger.WarningS("SightFearTraitSystem", $"Entity {uid} has SightFearedComponent without any defined fears.");
            return;
        }

        // Check if the RandomFears prototype exists
        if (!_prototype.TryIndex<WeightedRandomPrototype>("RandomFears", out var randomFears))
        {
            Logger.ErrorS("SightFearTraitSystem", $"Prototype RandomFears could not be found.");
            return;
        }

        // Check if the fears defined on the entity are in the RandomFears prototype
        foreach (var fear in component.Fears.Keys.Where(fear => !randomFears.Weights.ContainsKey(fear)))
        {
            Logger.ErrorS("SightFearTraitSystem", $"Prototype RandomFears does not contain fear {fear} from SightFearedComponent on entity {uid}.");
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Loop through all entities with SightFearTraitComponent
        var query = EntityQueryEnumerator<SightFearTraitComponent>();
        while(query.MoveNext(out var uid, out var component))
        {
            // Await the accumulator
            if (component.Accumulator > 0f)
            {
                component.Accumulator -= frameTime;
                continue;
            }

            // Reset the accumulator
            component.Accumulator = 0.084f;

            // Get the range the entity can see based on their eye component
            var range = 10f;
            if (_entity.TryGetComponent<SharedEyeComponent>(uid, out var eye))
                range *= (eye.Zoom.X + eye.Zoom.Y) / 2;

            // Get all entities in range that have a SightFearedComponent
            var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range)
                .Where(e => _entity.HasComponent<SightFearedComponent>(e));

            var afraid = false;

            // Loop through all fear inflicters
            foreach (var entity in entities)
            {
                // Check if the fear inflicter is in range and unoccluded (visible)
                if (!_entity.TryGetComponent<ExaminerComponent>(uid, out var examiner) ||
                    !examiner.InRangeUnOccluded(Transform(entity).Coordinates, range))
                    continue;

                // Check if the afraid should be afraid of the fear inflicter
                var feared = _entity.GetComponent<SightFearedComponent>(entity);
                if (!feared.Fears.TryGetValue(component.AfraidOf, out var value))
                    continue;

                // Calculate the strength of the fear
                var distance = (Transform(uid).Coordinates.Position - Transform(entity).Coordinates.Position).Length;
                var strength = MathHelper.Lerp(0f, value, 1f - distance / range);

                if (strength <= 0f || component.Fear >= component.MaxFear * 2)
                    continue;

                // Increase the level of fear
                afraid = true;
                component.Fear += strength;

                // TODO: Do something when afraid
                Logger.ErrorS("SightFearTraitSystem", $"Entity {uid} is afraid of {entity} ({component.AfraidOf}) at strength {strength}, now at a fear level of {component.Fear}/{component.MaxFear}.");
            }

            component.Afraid = afraid;

            // Decrease the fear level if not afraid this frame
            if (!afraid && component.Fear > 0f)
            {
                component.Fear -= frameTime * 1.19047619 * component.CalmRate; // Don't ask about the number.
                Logger.ErrorS("SightFearTraitSystem", $"Entity {uid} is not afraid, decreasing fear level to {component.Fear}/{component.MaxFear}.");
            }

            // Clamp the fear level
            component.Fear = Math.Clamp(component.Fear, 0, component.MaxFear * 2);
        }
    }
}
