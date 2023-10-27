using Content.Server.Power.Components;
using Content.Server.SimpleStation14.Power.Components;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Power.Systems;

public sealed class VariablePowerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VariablePowerComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VariablePowerComponent, VariablePowerPulsingComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var varPowerComp, out var powerPulseComp, out var apcPowerComp))
        {
            if (powerPulseComp.PulseDoneTime <= _timing.CurTime)
                PowerPulseDone(uid, varPowerComp, powerPulseComp, apcPowerComp);
        }
    }

    /// <summary>
    ///     Sets the power usage state of a device to either active (standard power draw) or inactive (idle multiplied power draw), as specified in the <see cref="VariablePowerComponent"/>.
    /// </summary>
    /// <param name="uid">The device to modify the power draw of.</param>
    /// <param name="active">Whether the device should be active or not.</param>
    /// <param name="apcPowerComp">The <see cref="ApcPowerReceiverComponent"/> of the device, if available.</param>
    /// <param name="varPowerComp">The <see cref="VariablePowerComponent"/> of the device, if available.</param>
    /// <remarks>
    ///     Won't do anything if the entity doesn't have a <see cref="VariablePowerComponent"/>, so can be called safely.
    /// </remarks>
    public void SetActive(EntityUid uid, bool active, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp) || !Resolve(uid, ref varPowerComp))
            return;

        if (TryComp<VariablePowerPulsingComponent>(uid, out var pulseComp))
        {
            pulseComp.NonPulseState = active;
            return;
        }

        SetActiveInternal(apcPowerComp, varPowerComp, active);
    }

    /// <summary>
    ///     Causes the device to pulse in power, using the pulse power during the pulse time as specified in the
    ///     <see cref="VariablePowerComponent"/> before returning to its previous state automatically.
    /// </summary>
    /// <inheritdoc cref="SetActive"/>
    public void DoPowerPulse(EntityUid uid, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp) || !Resolve(uid, ref varPowerComp))
            return;

        if (TryComp<VariablePowerPulsingComponent>(uid, out var pulseComp))
        {
            pulseComp.PulseDoneTime = _timing.CurTime + varPowerComp.PulseTime;
            return;
        }

        SetActiveInternal(apcPowerComp, varPowerComp, true, true);
        AddComp<VariablePowerPulsingComponent>(uid).PulseDoneTime = _timing.CurTime + varPowerComp.PulseTime;
    }

    /// <summary>
    ///     Changes the base power draw of the device (when under load) to the specified value.
    /// </summary>
    /// <param name="newBaseLoad">The new base power draw of the device.</param>
    /// <remarks>
    ///     Mostly used for compatibility with the UpgradePowerSystem.
    /// </remarks>
    /// <inheritdoc cref="SetActive"/>
    public void ChangeVariablePowerBaseLoad(EntityUid uid, float newBaseLoad, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp) || !Resolve(uid, ref varPowerComp))
            return;

        varPowerComp.ActiveLoad = newBaseLoad;
        SetActive(uid, varPowerComp.Active, null, varPowerComp);
    }

    private void PowerPulseDone(EntityUid uid, VariablePowerComponent varPowerComp, VariablePowerPulsingComponent pulseComp, ApcPowerReceiverComponent apcPowerComp)
    {
        SetActiveInternal(apcPowerComp, varPowerComp, pulseComp.NonPulseState); // Set the power state to whatever it was before the pulse.

        RemComp<VariablePowerPulsingComponent>(uid);
    }

    private void SetActiveInternal(ApcPowerReceiverComponent apcPowerComp, VariablePowerComponent varPowerComp, bool active, bool pulse = false)
    {
        varPowerComp.Active = active;
        apcPowerComp.Load = !active ? varPowerComp.ActiveLoad * varPowerComp.IdleMulti : pulse ? varPowerComp.PulseLoad : varPowerComp.ActiveLoad; // If not active > idle power, if pulse > pulse power, if active > active pwoer.
    }

    private void OnInit(EntityUid uid, VariablePowerComponent varPowerComp, ComponentInit args)
    {
        SetActive(uid, varPowerComp.Active, null, varPowerComp);
    }
}
