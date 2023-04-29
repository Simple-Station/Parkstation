using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Strip.Components;
using Content.Shared.Throwing;

namespace Content.Shared.SimpleStation14.Interaction;

public sealed class CantInteractSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CantInteractComponent, ThrowAttemptEvent>(OnThrowEvent);
        SubscribeLocalEvent<CantInteractComponent, PickupAttemptEvent>(OnPickupEvent);
        SubscribeLocalEvent<CantInteractComponent, DropAttemptEvent>(OnDropEvent); // stupid
        SubscribeLocalEvent<CantInteractComponent, InteractionAttemptEvent>(OnInteractEvent);
        SubscribeLocalEvent<CantInteractComponent, UseAttemptEvent>(OnUseEvent); // stupid

        SubscribeLocalEvent<CantInteractComponent, StrippingSlotButtonPressed>(OnStripEvent);
    }

    private static void OnThrowEvent(EntityUid uid, CantInteractComponent component, ThrowAttemptEvent args)
    {
        // If disallowed, prevent the action.
        if (!IsAllowed(component, args.ItemUid))
        {
            args.Cancel();
        }
    }

    private static void OnPickupEvent(EntityUid uid, CantInteractComponent component, PickupAttemptEvent args)
    {
        // If disallowed, prevent the action.
        if (!IsAllowed(component, args.Item))
        {
            args.Cancel();
        }
    }

    private static void OnDropEvent(EntityUid uid, CantInteractComponent component, DropAttemptEvent args)
    {
        // If disallowed, prevent the action.
        if (!IsAllowed(component, args.Uid))
        {
            args.Cancel();
        }
    }

    private static void OnInteractEvent(EntityUid uid, CantInteractComponent component, InteractionAttemptEvent args)
    {
        if (args.Target == null)
        {
            return;
        }

        // If disallowed, prevent the action.
        if (!IsAllowed(component, args.Target.Value))
        {
            args.Cancel();
        }
    }

    private static void OnUseEvent(EntityUid uid, CantInteractComponent component, UseAttemptEvent args)
    {
        // If disallowed, prevent the action.
        if (!IsAllowed(component, args.Uid))
        {
            args.Cancel();
        }
    }


    private static void OnStripEvent(EntityUid uid, CantInteractComponent component, StrippingSlotButtonPressed args)
    {
        // If disallowed, prevent the action.
        if (!IsAllowed(component, uid))
        {
            return; // ???? this seems to stop it from finishing the event for some reason
        }
    }


    /// <summary>
    ///     If no unless, then false.
    ///     If unless is valid, then true.
    ///     If unless is invalid, then false.
    /// </summary>
    /// <param name="component">Component to get the whitelist from</param>
    /// <param name="uid">Entity to check the whitelist against</param>
    /// <returns>Whether or not the action is allowed</returns>
    private static bool IsAllowed(CantInteractComponent component, EntityUid uid)
    {
        if (component.Unless == null)
            return false;

        return component.Unless.IsValid(uid);
    }
}
