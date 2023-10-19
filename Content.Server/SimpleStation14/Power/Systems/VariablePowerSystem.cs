using Content.Server.Power.Components;
using Content.Server.SimpleStation14.Power.Components;
using Robust.Shared.Timing;

namespace Content.Server.SimpleStation14.Power.Systems;

public sealed class VariablePowerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VariablePowerPulsingComponent>();
        while (query.MoveNext(out var uid, out var powerPulseComp))
        {
            if (powerPulseComp.PulseDoneTime <= _timing.CurTime)
                PowerPulseDone(uid, powerPulseComp);
        }
    }

    public void SetActive(EntityUid uid, bool active, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp))
            return;
        if (!Resolve(uid, ref varPowerComp))
            varPowerComp = AddComp<VariablePowerComponent>(uid);

        if (TryComp<VariablePowerPulsingComponent>(uid, out var pulseComp))
        {
            pulseComp.NonPulseState = active;
            return;
        }

        SetActiveInternal(apcPowerComp, varPowerComp, active);
    }

    public void DoPowerPulse(EntityUid uid, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp))
            return;
        if (!Resolve(uid, ref varPowerComp))
            varPowerComp = AddComp<VariablePowerComponent>(uid);

        if (TryComp<VariablePowerPulsingComponent>(uid, out var pulseComp))
        {
            pulseComp.PulseDoneTime = _timing.CurTime + varPowerComp.PulseTime;
            return;
        }

        SetActiveInternal(apcPowerComp, varPowerComp, true, true);
        AddComp<VariablePowerPulsingComponent>(uid).PulseDoneTime = _timing.CurTime + varPowerComp.PulseTime;
    }

    private void PowerPulseDone(EntityUid uid, VariablePowerPulsingComponent pulseComp)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var apcPowerComp) || !TryComp<VariablePowerComponent>(uid, out var varPowerComp))
            return;

        SetActiveInternal(apcPowerComp, varPowerComp, pulseComp.NonPulseState);

        RemComp<VariablePowerPulsingComponent>(uid);
    }

    public void ChangeVariablePowerBaseLoad(EntityUid uid, float newBaseLoad, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp))
            return;
        if (!Resolve(uid, ref varPowerComp))
            varPowerComp = AddComp<VariablePowerComponent>(uid);

        varPowerComp.ActiveLoad = newBaseLoad;
        SetActive(uid, varPowerComp.Active, null, varPowerComp);
    }

    private void SetActiveInternal(ApcPowerReceiverComponent apcPowerComp, VariablePowerComponent varPowerComp, bool active, bool pulse = false)
    {
        varPowerComp.Active = active;
        apcPowerComp.Load = !active ? varPowerComp.ActiveLoad * varPowerComp.IdleMulti : pulse ? varPowerComp.PulseLoad : varPowerComp.ActiveLoad; // If not active > idle power, if pulse > pulse power, if active > active pwoer.
    }
}
