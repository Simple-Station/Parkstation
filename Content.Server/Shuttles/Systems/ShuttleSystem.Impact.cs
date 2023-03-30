using System.Linq;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Shared.Explosion;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    /// <summary>
    /// Minimum velocity difference between 2 bodies for a shuttle "impact" to occur.
    /// </summary>
    private const int MinimumImpactVelocity = 9;

    private readonly SoundCollectionSpecifier _shuttleImpactSound = new("ShuttleImpactSound");

    private void InitializeImpact()
    {
        SubscribeLocalEvent<ShuttleComponent, StartCollideEvent>(OnShuttleCollide);
    }

    private void OnShuttleCollide(EntityUid uid, ShuttleComponent component, ref StartCollideEvent args)
    {
        var ourBody = args.OurFixture.Body;
        var otherBody = args.OtherFixture.Body;

        if (!HasComp<ShuttleComponent>(otherBody.Owner))
            return;

        // TODO: Would also be nice to have a continuous sound for scraping.
        var ourXform = Transform(ourBody.Owner);

        if (ourXform.MapUid == null)
            return;

        var otherXform = Transform(otherBody.Owner);

        var ourPoint = ourXform.InvWorldMatrix.Transform(args.WorldPoint);
        var otherPoint = otherXform.InvWorldMatrix.Transform(args.WorldPoint);

        var ourVelocity = _physics.GetLinearVelocity(ourBody.Owner, ourPoint, ourBody, ourXform);
        var otherVelocity = _physics.GetLinearVelocity(otherBody.Owner, otherPoint, otherBody, otherXform);
        var jungleDiff = (ourVelocity - otherVelocity).Length;

        if (jungleDiff < MinimumImpactVelocity)
        {
            return;
        }

        var coordinates = new EntityCoordinates(ourXform.MapUid.Value, args.WorldPoint);
        var volume = MathF.Min(10f, 1f * MathF.Pow(jungleDiff, 0.5f) - 5f);
        var audioParams = AudioParams.Default.WithVariation(0.05f).WithVolume(volume);

        var sysMan = IoCManager.Resolve<IEntitySystemManager>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        protoMan.TryIndex<ExplosionPrototype>("MicroBomb", out var type);
        type ??= protoMan.EnumeratePrototypes<ExplosionPrototype>().FirstOrDefault();
        if (type == null)
        {
            throw new InvalidOperationException("Couldn't find any explosion prototypes while trying to explode shuttle collision.");
        }

        var transform = EntityManager.GetComponent<TransformComponent>(uid);

        // Explosion power is 100 * velocity difference, which should be 900-2000 with shuttle speed limitations.
        sysMan.GetEntitySystem<ExplosionSystem>().QueueExplosion(new MapCoordinates(args.WorldPoint, transform.MapID), type.ID, jungleDiff * 100, 5, 75);
        _audio.Play(_shuttleImpactSound, Filter.Pvs(coordinates, rangeMultiplier: 4f, entityMan: EntityManager), coordinates, true, audioParams);
    }
}
