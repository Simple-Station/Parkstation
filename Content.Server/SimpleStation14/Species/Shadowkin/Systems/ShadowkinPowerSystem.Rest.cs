using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Bed.Sleep;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinRestSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;

    private InstantAction _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        _action = new InstantAction(_prototype.Index<InstantActionPrototype>("ShadowkinRest"));

        SubscribeLocalEvent<ShadowkinRestPowerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowkinRestPowerComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ShadowkinRestPowerComponent, ShadowkinRestEvent>(Rest);
    }


    private void OnStartup(EntityUid uid, ShadowkinRestPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, _action, uid);
    }

    private void OnShutdown(EntityUid uid, ShadowkinRestPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, _action);
    }

    private void Rest(EntityUid uid, ShadowkinRestPowerComponent component, ShadowkinRestEvent args)
    {
        if (!_entity.HasComponent<ShadowkinComponent>(args.Performer))
            return;

        // Now doing what you weren't before
        component.IsResting = !component.IsResting;

        // Resting
        if (component.IsResting)
        {
            // Sleepy time
            _entity.EnsureComponent<ForcedSleepingComponent>(args.Performer);
            // No waking up normally (it would do nothing)
            _actions.RemoveAction(args.Performer, new InstantAction(_prototype.Index<InstantActionPrototype>("Wake")));
            _power.TryAddMultiplier(args.Performer, 1f);
            // No action cooldown
            args.Handled = false;
        }
        // Waking
        else
        {
            // Wake up
            _entity.RemoveComponent<ForcedSleepingComponent>(args.Performer);
            _entity.RemoveComponent<SleepingComponent>(args.Performer);
            _power.TryAddMultiplier(args.Performer, -1f);
            // Action cooldown
            args.Handled = true;
        }
    }
}
