using Content.Shared.Spawners.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Random;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Throwing;
using System.Threading.Tasks;
using Robust.Shared.Utility;
using System.Collections.Generic;

namespace Content.Server.SimpleStation14.Grenades;

public class SpawnEntitiesOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;

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
    private async void OnTrigger(EntityUid uid, SpawnEntitiesOnTriggerComponent component, TriggerEvent args)
    {
        if (component.Prototype == null) return;

        if (!_prototypeManager.TryIndex<EntityPrototype>(component.Prototype, out var prototype))
        {
            DebugTools.Assert($"Prototype {component.Prototype} does not exist");

            return;
        }

        var spawnCount = component.Count;

        if (component.MinCount != null)
        {
            spawnCount = _random.Next(component.MinCount.Value, component.Count);
        }

        var spawnedEntities = await SpawnEntities(spawnCount, prototype, component, uid, Transform(uid));

        foreach (var spawnedEntity in spawnedEntities)
        {
            if (component.DespawnTime != null)
            {
                var despawnComp = EntityManager.EnsureComponent<TimedDespawnComponent>(spawnedEntity);
                despawnComp.Lifetime = component.DespawnTime.Value;
            }
        }
    }

    /// <summary>
    ///     Async method to spawn the entities to prevent lag spikes.
    /// </summary>
    private async Task<List<EntityUid>> SpawnEntities(int spawnCount, EntityPrototype prototype, SpawnEntitiesOnTriggerComponent component, EntityUid uid, TransformComponent transfComp)
    {
        var spawnedEntities = new List<EntityUid>();

        for (var i = 0; i < spawnCount * 100000; i++)
        {
            if (i % 100000 != 0) continue;

            var spawnedEntity = EntityManager.SpawnEntity(prototype.ID, transfComp.Coordinates
                + new EntityCoordinates(transfComp.ParentUid, _random.NextVector2(-1, 1) * component.Offset));

            Transform(spawnedEntity).AttachToGridOrMap();

            // If the component has a velocity set, prepare to launch the shrapnel.
            if (component.Velocity != null)
            {
                var physicsComp = EntityManager.GetComponent<PhysicsComponent>(spawnedEntity);

                Transform(spawnedEntity).LocalRotation = Angle.FromDegrees(_random.Next(0, 360));

                // Decide to either shoot, or throw.
                if (component.Shoot)
                {
                    _gunSystem.ShootProjectile(spawnedEntity, Transform(spawnedEntity).LocalRotation.ToWorldVec(), Vector2.Zero, speed: component.Velocity.Value * _random.NextFloat(0.8f, 1.2f));
                }
                else
                {
                    _throwingSystem.TryThrow(spawnedEntity, Transform(spawnedEntity).LocalRotation.ToWorldVec(), 1.0f, null, 5.0f, physicsComp, Transform(spawnedEntity));
                }
            }

            spawnedEntities.Add(spawnedEntity);
        }

        return spawnedEntities;
    }
}
