using Content.Shared.SimpleStation14.Traits.SightFear;
using Robust.Shared.GameStates;
using Robust.Shared.Random;

namespace Content.Shared.SimpleStation14.Traits.SightFear;

public sealed class SightFearTraitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SightFearTraitComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<SightFearTraitComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<SightFearTraitComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnInit(EntityUid entity, SightFearTraitComponent traitComponent, ComponentInit args)
    {
        // If there are no possible fears, don't bother.
        if (traitComponent.PossiblePrototypes.Count <= 0 && traitComponent.PossibleTags.Count <= 0)
            return;

        // Randomly pick a fear.
        if (_random.Prob(0.5f))
            traitComponent.AfraidOfPrototypes.Add(_random.PickAndTake(traitComponent.PossiblePrototypes));
        else
            traitComponent.AfraidOfTags.Add(_random.PickAndTake(traitComponent.PossibleTags));

        RecursiveRandom(traitComponent.MultipleFearChance, traitComponent);
    }


    private void OnGetState(EntityUid entity, SightFearTraitComponent traitComponent, ref ComponentGetState args)
    {
        args.State = new SightFearTraitComponentState(traitComponent.AfraidOfPrototypes, traitComponent.AfraidOfTags);
    }

    private void OnHandleState(EntityUid entity, SightFearTraitComponent traitComponent, ref ComponentHandleState args)
    {
        if (args.Current is not SightFearTraitComponentState state)
            return;

        traitComponent.AfraidOfPrototypes = state.AfraidOfPrototypes;
        traitComponent.AfraidOfTags = state.AfraidOfTags;
    }


    private void RecursiveRandom(float chance, SightFearTraitComponent traitComponent)
    {
        if (traitComponent.PossiblePrototypes.Count <= 0 && traitComponent.PossibleTags.Count <= 0)
            return;

        if (_random.Prob(0.5f) && traitComponent.PossiblePrototypes.Count > 0)
        {
            if (!_random.Prob(chance))
                return;

            traitComponent.AfraidOfPrototypes.Add(_random.PickAndTake(traitComponent.PossiblePrototypes));
            RecursiveRandom(chance, traitComponent);
            return;
        }

        if (traitComponent.PossibleTags.Count > 0)
        {
            if (!_random.Prob(chance))
                return;

            traitComponent.AfraidOfTags.Add(_random.PickAndTake(traitComponent.PossibleTags));
            RecursiveRandom(chance, traitComponent);
            return;
        }
    }


    // public override void Update(float frameTime)
    // {
    //     base.Update(frameTime);
    //
    //     if (!_timing.IsFirstTimePredicted)
    //         return;
    //
    //     var player = _player.LocalPlayer?.ControlledEntity;
    //     if (player == null)
    //         return;
    //
    //     if (!_entity.TryGetComponent<SightFearTraitComponent>(player, out var fear))
    //         return;
    // }
}
