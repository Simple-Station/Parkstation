using Content.Server.Body.Components;
using Content.Server.DoAfter;
using Content.Server.SimpleStation14.Silicon.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.SimpleStation14.Silicon.Components;

using Content.Server.Body.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;

namespace Content.Server.SimpleStation14.Silicon.Systems;

public sealed class BloodstreamFillerSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly BloodstreamSystem _bloodSystem = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamFillerComponent, AfterInteractEvent>(OnUseInWorld);
        SubscribeLocalEvent<BloodstreamFillerComponent, UseInHandEvent>(OnUseInHand);

        SubscribeLocalEvent<BloodstreamFillerComponent, DoAfterEvent>(OnDoAfter);
    }

    private void OnUseInWorld(EntityUid uid, BloodstreamFillerComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        if (!EntityManager.TryGetComponent<BloodstreamComponent>(args.Target, out var bloodComp))
        {
            TryRefill(uid, args.Target.Value, component);
            return;
        }

        TryFill(args.User, args.Target.Value, args.Used, component);
    }

    private void OnUseInHand(EntityUid uid, BloodstreamFillerComponent component, UseInHandEvent args)
    {
        if (!EntityManager.TryGetComponent<BloodstreamComponent>(args.User, out var bloodComp))
            return;

        TryFill(args.User, args.User, uid, component);
    }

    private void TryFill(EntityUid user, EntityUid target, EntityUid filler, BloodstreamFillerComponent fillComp)
    {
        var bloodComp = EntityManager.GetComponent<BloodstreamComponent>(target);

        if (!_solutionSystem.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution) ||
            fillerSolution.Contents.Count != 1 || // Extra dorty
            fillerSolution.Contents[0].ReagentId != bloodComp.BloodReagent)
            return;

        var delay = 2.5f;
        if (user == target)
            delay *= 3.5f;

        _doAfter.DoAfter(new DoAfterEventArgs(user, delay, target: target, used: filler)
        {
            RaiseOnTarget = true,
            RaiseOnUser = false,
            BreakOnUserMove = true,
            BreakOnDamage = true,
            BreakOnStun = true,
            BreakOnTargetMove = true,
            MovementThreshold = 0.2f
        });
    }

    private void OnDoAfter(EntityUid uid, BloodstreamFillerComponent component, DoAfterEvent args)
    {
        if (args.Cancelled)
        {
            return;
        }

        if (args.Handled || args.Args.Target == null)
            return;

        Fill(args.Args.Target.Value, args.Args.Used!.Value, component);

        args.Handled = true;
    }

    private void Fill(EntityUid target, EntityUid filler, BloodstreamFillerComponent fillComp)
    {
        if (!_solutionSystem.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution))
            return;

        var bloodComp = EntityManager.GetComponent<BloodstreamComponent>(target);

        var tansfAmount = FixedPoint2.Min(bloodComp.BloodSolution.AvailableVolume, Math.Min((float) fillerSolution.Volume, fillComp.Amount));

        if (tansfAmount > 0)
        {
            var drained = _solutionSystem.SplitSolution(filler, fillerSolution, tansfAmount);

            _bloodSystem.TryModifyBloodLevel(target, drained.Volume, bloodComp);
            // _audioSystem.PlayPvs(welder.WelderRefill, welderUid);
            // _popupSystem.PopupCursor(Loc.GetString("welder-component-after-interact-refueled-message"), user);
        }
    }

    private void TryRefill(EntityUid filler, EntityUid target, BloodstreamFillerComponent fillComp)
    {
        if (!EntityManager.TryGetComponent<ReagentTankComponent>(target, out var tankComp))
            return;

        // Check that the tank has one, and only one reagent.
        if (!_solutionSystem.TryGetDrainableSolution(target, out var targetSolution)||
            targetSolution.Contents.Count > 1) // Dorty
            return;

        // Check that the filler has one, and only one reagent, and that it's the same as the tank.
        if (!_solutionSystem.TryGetSolution(filler, (string) fillComp.Solution!, out var fillerSolution) ||
            fillerSolution.Contents.Count > 1 || // Extra dorty
            (fillerSolution.Contents.Count > 0 && fillerSolution.Contents[0].ReagentId != targetSolution.Contents[0].ReagentId))
            return;

        var tansfAmount = FixedPoint2.Min(fillerSolution.AvailableVolume, targetSolution.Volume);
        if (tansfAmount > 0)
        {
            var drained = _solutionSystem.Drain(target, targetSolution,  tansfAmount);
            _solutionSystem.TryAddSolution(filler, fillerSolution, drained);
            // _audioSystem.PlayPvs(welder.WelderRefill, welderUid);
            // _popupSystem.PopupCursor(Loc.GetString("welder-component-after-interact-refueled-message"), user);
        }
        else if (fillerSolution.AvailableVolume <= 0)
        {
            // _popupSystem.PopupCursor(Loc.GetString("welder-component-already-full"), user);
        }
        else
        {
            // _popupSystem.PopupCursor(Loc.GetString("welder-component-no-fuel-in-tank", ("owner", target)), user);
        }
    }
}
