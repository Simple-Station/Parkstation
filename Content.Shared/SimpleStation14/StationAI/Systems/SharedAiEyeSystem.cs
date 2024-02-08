using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.SimpleStation14.StationAI;
using Robust.Shared.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Shared.Borgs;
using Content.Shared.SimpleStation14.Holograms.Components;
using Content.Shared.SimpleStation14.Holograms;
using Robust.Shared.Timing;

namespace Content.Shared.SimpleStation14.StationAI.Systems;

public sealed class SharedAiEyeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedHologramSystem _hologramSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AIEyeComponent, HologramGetProjectorEvent>(OnHologramGetProjector);
        SubscribeLocalEvent<AIEyeComponent, HologramCheckProjectorValidEvent>(OnHologramCheckProjectorValid);
    }

    private void OnHologramGetProjector(EntityUid eyeUid, AIEyeComponent eyeComp, ref HologramGetProjectorEvent args)
    {
        if (!TryComp<HologramProjectedComponent>(eyeUid, out var projectedComp))
            return;

        if (!_hologramSystem.IsHoloProjectorValid(eyeUid, projectedComp.CurProjector, projectedComp: projectedComp))
            return;


    }

    private void OnHologramCheckProjectorValid(EntityUid eyeUid, AIEyeComponent eyeComp, ref HologramCheckProjectorValidEvent args)
    {
        if (!HasComp<AIEyePowerComponent>(args.Projector) || !TryComp<HologramServerLinkedComponent>(eyeUid, out var serverLinkedComp) || !_hologramSystem.IsHoloProjectorValid(eyeUid, args.Projector, raiseEvent: false))
            return;

        if (serverLinkedComp.LinkedServer != args.Projector)
        {
            Log.Error($"Projector {args.Projector} is not valid for eye {eyeUid}");
            args.Valid = false;
        }

    }
}
