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

        SubscribeLocalEvent<VariablePowerComponent, ComponentInit>(OnVariablePowerInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VariablePowerPulsingComponent>();
        while (query.MoveNext(out var uid, out var powerPulseComp))
        {
            if (powerPulseComp.PulseDoneTime <= _timing.CurTime)
            {
                SetActive(uid, false);
                RemComp<VariablePowerPulsingComponent>(uid);
            }
        }
    }

    public void SetActive(EntityUid uid, bool active, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        if (!Resolve(uid, ref apcPowerComp))
            return;
        if (!Resolve(uid, ref varPowerComp))
            varPowerComp = EnsureComp<VariablePowerComponent>(uid);

        if (active == varPowerComp.Active)
            return;

        varPowerComp.Active = active;
        if (active)
        {
            var addition = apcPowerComp.Load * varPowerComp.PowerActiveMulti - apcPowerComp.Load;
            apcPowerComp.Load += addition;
            varPowerComp.AddedValue = addition;
        }
        else
        {
            apcPowerComp.Load -= varPowerComp.AddedValue;
            varPowerComp.AddedValue = 0f;
        }
    }

    public void DoPowerPulse(EntityUid uid, ApcPowerReceiverComponent? apcPowerComp = null, VariablePowerComponent? varPowerComp = null)
    {
        SetActive(uid, true, apcPowerComp, varPowerComp);

        EnsureComp<VariablePowerPulsingComponent>(uid);
    }

    private void OnVariablePowerInit(EntityUid uid, VariablePowerComponent component, ComponentInit args)
    {
        SetActive(uid, false); // This might not be perfect, but the given system can deal with that.
    }
}
