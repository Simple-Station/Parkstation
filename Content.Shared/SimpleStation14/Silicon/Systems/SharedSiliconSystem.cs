using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.Alert;
using Robust.Shared.Serialization;
using Content.Shared.Movement.Systems;

namespace Content.Shared.SimpleStation14.Silicon.Systems;

public sealed class SharedSiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    // Dictionary of ChargeState to Alert severity.
    private static readonly Dictionary<ChargeState, short> ChargeStateAlert = new()
    {
        {ChargeState.Full, 4},
        {ChargeState.Mid, 3},
        {ChargeState.Low, 2},
        {ChargeState.Critical, 1},
        {ChargeState.Dead, 0},
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentInit>(OnSiliconInit);
        SubscribeLocalEvent<SiliconComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
        SubscribeLocalEvent<SiliconComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
    }

    private void OnSiliconInit(EntityUid uid, SiliconComponent component, ComponentInit args)
    {
        component.Owner = uid;

        if (component.BatteryPowered)
        {
            _alertsSystem.ShowAlert(uid, AlertType.Charge, ChargeStateAlert[component.ChargeState]);
        }
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconComponent component, SiliconChargeStateUpdateEvent ev)
    {
        _alertsSystem.ShowAlert(uid, AlertType.Charge, ChargeStateAlert[component.ChargeState]);
    }

    private void OnRefreshMovespeed(EntityUid uid, SiliconComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!component.BatteryPowered)
            return;

        var speedModThresholds = component.SpeedModifierThresholds;

        var closest = -0.5f;

        foreach (var state in speedModThresholds)
        {
            if (component.ChargeState >= state.Key && (float) state.Key > closest)
                closest = (float) state.Key;
        }

        var speedMod = speedModThresholds[(ChargeState) closest];

        args.ModifySpeed(speedMod, speedMod);
    }
}


public enum SiliconType
{
    Player,
    GhostRole,
    NPC
}

public enum ChargeState
{
    Full,
    Mid,
    Low,
    Critical,
    Dead
}


/// <summary>
///     Event raised when a Silicon's charge state needs to be updated.
/// </summary>
[Serializable, NetSerializable]
public sealed class SiliconChargeStateUpdateEvent : EntityEventArgs
{
    public ChargeState ChargeState { get; }

    public SiliconChargeStateUpdateEvent(ChargeState chargeState)
    {
        ChargeState = chargeState;
    }
}
