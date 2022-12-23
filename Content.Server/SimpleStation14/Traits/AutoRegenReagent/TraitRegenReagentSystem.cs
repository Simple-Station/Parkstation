using Content.Server.Chemistry.EntitySystems;
using Content.Server.Popups;

namespace Content.Server.SimpleStation14.Traits
{
    public sealed class TraitRegenReagentSsytem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var autoComp in EntityQuery<TraitRegenReagentComponent>())
            {
                if (TryComp<DraconicBloodstreamComponent>(autoComp.Owner, out var Draconic) && !autoComp.Reagents.Contains(Draconic.Reagent))
                    autoComp.Reagents.Add(Draconic.Reagent);
                if (TryComp<EasyDrunkComponent>(autoComp.Owner, out var EasyDrunk) && !autoComp.Reagents.Contains(EasyDrunk.Reagent))
                    autoComp.Reagents.Add(EasyDrunk.Reagent);

                foreach (var regens in autoComp.Reagents)
                {
                    regens.Accumulator += frameTime;
                    if (regens.Accumulator < regens.AccumulatorTime) continue;
                    regens.Accumulator -= regens.AccumulatorTime;

                    if (_solutionSystem.TryGetSolution(autoComp.Owner, regens.solutionName, out var solution)) regens.solution = solution;

                    if (regens.solution == null) continue;
                    _solutionSystem.TryAddReagent(autoComp.Owner, regens.solution, regens.reagent, regens.unitsPerUpdate, out var accepted);
                }
            }
        }
    }
}
