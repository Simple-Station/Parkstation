using System.Linq;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.SimpleStation14.Chemistry.Components;
using Robust.Shared.Physics.Events;

namespace Content.Server.SimpleStation14.Chemistry.Systems;

public sealed class SpikeOnCollideSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SolutionContainerSystem _solution = default!;
    [Dependency] private readonly SolutionSpikableSystem _spikable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikeOnCollideComponent, StartCollideEvent>(OnCollide);
    }


    private void OnCollide(EntityUid uid, SpikeOnCollideComponent component, ref StartCollideEvent args)
    {
        Logger.Error($"SpikeOnCollideSystem: {uid} collided with {args.OtherFixture.Body.Owner}");

        if (_entity.TryGetComponent<SolutionContainerManagerComponent>(args.OtherFixture.Body.Owner, out var _))
        {
            Logger.Error($"SpikeOnCollideSystem: {uid} collided with {args.OtherFixture.Body.Owner} and it has a SolutionContainerManagerComponent");

            _entity.TryGetComponent(uid, out RefillableSolutionComponent? spikableTarget);
            _entity.TryGetComponent(args.OtherFixture.Body.Owner, out SolutionSpikerComponent? spikableSource);
            _entity.TryGetComponent(args.OtherFixture.Body.Owner, out SolutionContainerManagerComponent? managerSource);
            _entity.TryGetComponent(uid, out SolutionContainerManagerComponent? managerTarget);

            _spikable.TrySpike(args.OtherFixture.Body.Owner, uid, args.OtherFixture.Body.Owner, spikableTarget, spikableSource, managerSource, managerTarget);
        }
    }
}
