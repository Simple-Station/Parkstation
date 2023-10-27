using Content.Server.SimpleStation14.Power.Components;
using Content.Shared.Doors;
using Content.Shared.Doors.Components;

namespace Content.Server.SimpleStation14.Power.Systems;

public sealed class VariablePowerDoorSystem : EntitySystem
{
    [Dependency] private readonly VariablePowerSystem _variablePower = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VariablePowerComponent, DoorStateChangedEvent>(OnDoorStateChanged);
    }

    private void OnDoorStateChanged(EntityUid uid, VariablePowerComponent varPowerComp, DoorStateChangedEvent args)
    {
        _variablePower.SetActive(uid, args.State switch { DoorState.Opening => true, DoorState.Closing => true, _ => false });
    }
}
