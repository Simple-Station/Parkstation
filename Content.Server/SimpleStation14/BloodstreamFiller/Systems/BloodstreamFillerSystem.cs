using Content.Server.Body.Components;
using Content.Server.SimpleStation14.BloodstreamFiller.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Server.Body.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.SimpleStation14.BloodstreamFiller;
using Robust.Shared.Utility;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Server.Fluids.EntitySystems;

namespace Content.Server.SimpleStation14.BloodstreamFiller.Systems;

public sealed class BloodstreamFillerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly SolutionContainerSystem _solution = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamFillerComponent, AfterInteractEvent>(OnUseInWorld);
        SubscribeLocalEvent<BloodstreamFillerComponent, UseInHandEvent>(OnUseInHand);

        SubscribeLocalEvent<BloodstreamComponent, BloodstreamFillerDoAfterEvent>(OnDoAfter);
    }

    private void OnUseInWorld(EntityUid uid, BloodstreamFillerComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        if (!EntityManager.TryGetComponent<BloodstreamComponent>(args.Target, out var bloodComp))
        {
            TryRefill(args.User, uid, args.Target.Value, component);
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
        // if (fillComp.SiliconOnly && !HasComp<SiliconComponent>(target)) // To be turned on once Silicons are merged :)
        // {
        //     // Do failure stuff
        //     return;
        // }

        var bloodComp = EntityManager.GetComponent<BloodstreamComponent>(target);

        if (!_solution.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution)) // No solution
            return;

        if (fillComp.Reagent != null && fillComp.Reagent != bloodComp.BloodReagent) // Wrong reagent as specified by the component
        {
            _popup.PopupCursor(Loc.GetString(fillComp.TargetInvalidPopup, ("filler", filler)), user);
            return;
        }

        if (fillerSolution.Contents.Count == 0) // Empty
        {
            _popup.PopupCursor(Loc.GetString(fillComp.EmptyPopup, ("filler", filler)), user);
            return;
        }

        if (fillerSolution.Contents.Count > 1) // Extra dorty
        {
            _popup.PopupCursor(Loc.GetString(fillComp.DirtyPopup, ("volume", filler)), user);
            return;
        }

        if (fillerSolution.Contents[0].ReagentId != bloodComp.BloodReagent) // Wrong reagent contained
        {
            _popup.PopupCursor(Loc.GetString(fillComp.TargetBloodInvalidPopup, ("filler", filler), ("target", target)), user);
            return;
        }

        var overfill = false;

        var delay = fillComp.FillTime;
        if (user == target)
            delay *= fillComp.SelfFillMutli;

        // If the bloodstream is already full, and the filler can overfill, and target is not the user, then overfill.
        if (fillComp.Overfill && bloodComp.BloodSolution.AvailableVolume == 0 && user != target)
        {
            overfill = true;
            delay *= fillComp.OverfillMutli;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(user, delay, new BloodstreamFillerDoAfterEvent(overfill), target, target, filler)
        {
            BreakOnDamage = true,
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
            MovementThreshold = 0.2f,
            CancelDuplicate = true
        });

        if (!overfill)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.UsePopup, ("target", target)), user);
            _popup.PopupEntity(Loc.GetString(fillComp.UsedOnPopup), target, target, PopupType.Medium);
        }
        else
        {
            _popup.PopupCursor(Loc.GetString(fillComp.TargetOverfillPopup, ("target", target)), user, PopupType.MediumCaution);
            _popup.PopupEntity(Loc.GetString(fillComp.OverfilledPopup), target, target, PopupType.LargeCaution);
        }
    }

    private void OnDoAfter(EntityUid uid, BloodstreamComponent component, BloodstreamFillerDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            return;
        }

        if (args.Handled || args.Args.Target == null)
            return;

        if (!TryComp<BloodstreamFillerComponent>(args.Args.Used, out var fillComp))
        {
            DebugTools.Assert("Filler component not found");
            Logger.ErrorS("silicon", $"Filler component not found on entity {ToPrettyString(args.Args.Used.Value)}");

            return;
        }
        if (!EntityManager.TryGetComponent<BloodstreamComponent>(args.Args.Target, out var bloodComp))
        {
            DebugTools.Assert("Bloodstream component not found");
            Logger.ErrorS("silicon", $"Bloodstream component not found on entity {ToPrettyString(args.Args.Target.Value)}");

            return;
        }

        if (!args.Overfill)
            Fill(args.Args.Target.Value, args.Args.Used!.Value, fillComp, bloodComp);
        else
            Overfill(args.Args.User, args.Args.Target.Value, args.Args.Used!.Value, fillComp, bloodComp);

        args.Handled = true;
    }

    private void Fill(EntityUid target, EntityUid filler, BloodstreamFillerComponent fillComp, BloodstreamComponent bloodComp)
    {
        if (!_solution.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution))
            return;

        var tansfAmount = FixedPoint2.Min(bloodComp.BloodSolution.AvailableVolume, Math.Min((float) fillerSolution.Volume, fillComp.Amount));

        if (tansfAmount > 0)
        {
            var drained = _solution.SplitSolution(filler, fillerSolution, tansfAmount);

            _bloodstream.TryModifyBloodLevel(target, drained.Volume, bloodComp);

            _audio.PlayPvs(fillComp.RefillSound, filler);
        }
    }

    private void Overfill(EntityUid user, EntityUid target, EntityUid filler, BloodstreamFillerComponent fillComp, BloodstreamComponent bloodComp)
    {
        if (!_solution.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution))
            return;

        if (!TryComp<DamageableComponent>(target, out var damageableComp))
            return;

        _damageable.TryChangeDamage(target, fillComp.OverfillDamage, origin: user, damageable: damageableComp);

        _puddle.TrySplashSpillAt(target, Transform(target).Coordinates, fillerSolution, out _, user: user);

        Fill(target, filler, fillComp, bloodComp);
    }

    private void TryRefill(EntityUid user, EntityUid filler, EntityUid target, BloodstreamFillerComponent fillComp)
    {
        if (!EntityManager.TryGetComponent<ReagentTankComponent>(target, out _))
            return;

        if (!_solution.TryGetDrainableSolution(target, out var targetSolution))
            return;

        if (!_solution.TryGetSolution(filler, fillComp.Solution!, out var fillerSolution))
            return;

        // Check that the tank is not empty.
        if (targetSolution.Contents.Count == 0)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.RefillTankEmptyPopup, ("tank", target)), user);
            return;
        }

        // Check that the tank has one, and only one reagent.
        if (targetSolution.Contents.Count > 1)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.DirtyPopup, ("volume", target)), user);
            return;
        }

        // Check that the tank's solution matches the filler's listed reagent.
        // This is seperate from checking the actual solution to prevent any funny business.
        if (fillComp.Reagent != null && targetSolution.Contents[0].ReagentId != fillComp.Reagent)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.RefillReagentInvalidPopup, ("tank", target)), user);
            return;
        }

        // Check that if the filler isn't empty, that it only has one reagent.
        if (fillerSolution.Contents.Count > 1)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.DirtyPopup, ("volume", filler)), user);
            return;
        }

        // Check that if the filler isn't empty, that it's reagent matches the tank's reagent.
        if (fillerSolution.Contents.Count == 1 && fillerSolution.Contents[0].ReagentId != targetSolution.Contents[0].ReagentId)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.RefillReagentInvalidPopup, ("filler", filler)), user);
            return;
        }

        var tansfAmount = FixedPoint2.Min(fillerSolution.AvailableVolume, targetSolution.Volume);

        if (tansfAmount > 0)
        {
            var drained = _solution.Drain(target, targetSolution, tansfAmount);
            _solution.TryAddSolution(filler, fillerSolution, drained);

            _audio.PlayPvs(fillComp.UseSound, filler);
        }
        else if (fillerSolution.AvailableVolume <= 0)
        {
            _popup.PopupCursor(Loc.GetString(fillComp.RefillFullPopup, ("filler", filler)), user);
        }
        else
        {
            _popup.PopupCursor(Loc.GetString(fillComp.RefillTankEmptyPopup, ("tank", target)), user);
        }
    }
}
