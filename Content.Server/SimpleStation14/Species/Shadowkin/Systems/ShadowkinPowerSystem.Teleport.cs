using Content.Server.Pulling;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Pulling.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinTeleportSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private WorldTargetAction _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        _action = new WorldTargetAction(_prototype.Index<WorldTargetActionPrototype>("ShadowkinTeleport"));

        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentShutdown>(Shutdown);

        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ShadowkinTeleportEvent>(Teleport);
    }


    private void Startup(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, _action, uid);
    }

    private void Shutdown(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, _action);
    }


    private void Teleport(EntityUid uid, ShadowkinTeleportPowerComponent component, ShadowkinTeleportEvent args)
    {
        if (args.Handled ||
            !_entity.TryGetComponent<ShadowkinComponent>(args.Performer, out var comp))
            return;


        var transform = Transform(args.Performer);
        if (transform.MapID != args.Target.GetMapId(EntityManager))
            return;

        SharedPullableComponent? pullable = null; // To avoid "might not be initialized when accessed" warning
        if (_entity.TryGetComponent<SharedPullerComponent>(args.Performer, out var puller) &&
            puller.Pulling != null &&
            _entity.TryGetComponent<SharedPullableComponent>(puller.Pulling, out pullable) &&
            pullable.BeingPulled)
        {
            // Temporarily stop pulling to avoid not teleporting to the target
            _pulling.TryStopPull(pullable);
        }

        // Teleport the performer to the target
        _transform.SetCoordinates(args.Performer, args.Target);
        transform.AttachToGridOrMap();

        if (pullable != null && puller != null)
        {
            // Get transform of the pulled entity
            var pulledTransform = Transform(pullable.Owner);

            // Teleport the pulled entity to the target
            // TODO: Relative position to the performer
            _transform.SetCoordinates(pullable.Owner, args.Target);
            pulledTransform.AttachToGridOrMap();

            // Resume pulling
            // TODO: This does nothing?
            _pulling.TryStartPull(puller, pullable);
        }


        // Play the teleport sound
        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        // Take power and deal stamina damage
        _power.TryAddPowerLevel(comp.Owner, -args.PowerCost);
        _stamina.TakeStaminaDamage(args.Performer, args.StaminaCost);

        args.Handled = true;
    }
}
