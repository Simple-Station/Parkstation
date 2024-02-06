using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.SurveillanceCamera;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.SimpleStation14.Holograms.Components;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class HologramProjectorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent((EntityUid ent, HologramProjectorComponent comp, ref PowerChangedEvent _) => CheckState(ent, comp));
        SubscribeLocalEvent<HologramProjectorComponent, SurveillanceCameraChangeStateEvent>((ent, comp, args) => CheckState(ent, comp));
        SubscribeLocalEvent<HologramProjectorComponent, MapInitEvent>((ent, comp, args) => CheckState(ent, comp));
    }

    public void CheckState(EntityUid projector, HologramProjectorComponent? projComp = null)
    {
        if (!Resolve(projector, ref projComp))
            return;

        if (TryComp<ApcPowerReceiverComponent>(projector, out var powerComp) && !powerComp.Powered ||
            TryComp<SurveillanceCameraComponent>(projector, out var cameraComp) && !cameraComp.Active)
        {
            RemComp<HologramProjectorActiveComponent>(projector);
            return;
        }

        EnsureComp<HologramProjectorActiveComponent>(projector);
    }
}
