using Content.Shared.Spawners.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Random;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Grenades;

public class SpawnEntitiesOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnEntitiesOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    /// <summary>
    ///     On trigger, works out if the entity has a variable count, decides how many to spawn,
    ///     then iterates through and spawns each one apply any offset, and any velocity, be it thrown or shot,
    ///     spinning them in a random direction. Finally, sets the entity to be deleted if a despawn time is set.
    /// </summary>
    private void OnTrigger(EntityUid uid, SpawnEntitiesOnTriggerComponent component, TriggerEvent args)
    {
        if (component.Prototype == null) return;

        var prototype = _prototypes.Index<EntityPrototype>(component.Prototype);

        var spawnCount = component.Count;

        if (component.MinCount != null)
        {
            spawnCount = _random.Next(component.MinCount.Value, component.Count);
        }

        for (var i = 0; i < spawnCount; i++)
        {
            var spawnedEntity = EntityManager.SpawnEntity(prototype.ID, EntityManager.GetComponent<TransformComponent>(uid).Coordinates
                + new EntityCoordinates(EntityManager.GetComponent<TransformComponent>(uid).ParentUid, _random.NextVector2(-1, 1) * component.Offset));

            var transfComp = EntityManager.GetComponent<TransformComponent>(spawnedEntity);
            _transform.AttachToGridOrMap(spawnedEntity);

            if (component.Velocity != null)
            {
                transfComp.LocalRotation = Angle.FromDegrees(_random.Next(0, 360));

                if (component.Shoot)
                {
                    _guns.ShootProjectile(spawnedEntity, transfComp.LocalRotation.ToWorldVec(), Vector2.Zero, speed: component.Velocity.Value * _random.NextFloat(0.8f, 1.2f));
                }
                else
                {
                    _throw.TryThrow(spawnedEntity, transfComp.LocalRotation.ToWorldVec(), 1.0f, null, 5.0f);
                }
            }

            if (component.DespawnTime != null)
            {
                var despawnComp = EntityManager.EnsureComponent<TimedDespawnComponent>(spawnedEntity);
                despawnComp.Lifetime = component.DespawnTime.Value;
            }
        }
    }
}
