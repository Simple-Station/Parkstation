using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Weapons.Ranged.Systems;

public sealed class FireOnDropSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, LandEvent>(HandleEmitSoundOnDrop);
    }

    private void HandleEmitSoundOnDrop(EntityUid uid, GunComponent component, ref LandEvent args)
    {
        var physicsComp = EntityManager.GetComponent<PhysicsComponent>(uid);

        // TODO: This shouldn't be a hardcoded 10% roll. I wanted to base it off mass, but items don't seem
        // to care about their mass (most guns had the same.).
        // Then I wanted to base it off of item size, but the minigun still has a size of 5 at the time of writing, so :shrug:
        if (_random.Prob(0.1f))
            _gun.AttemptShoot(uid, uid, component, Transform(uid).Coordinates.Offset(Transform(uid).LocalRotation.ToVec()));
        // The gun fires itself (weird), with the target being its own position offset by its rotation as a point vector.
        // The result being that it will always fire the direction that all gun sprites point in.
    }
}
