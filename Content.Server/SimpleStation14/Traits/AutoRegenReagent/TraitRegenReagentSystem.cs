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
                foreach (var regens in autoComp.Reagents)
                {
                    regens.Accumulator += frameTime;
                    if (regens.Accumulator < regens.AccumulatorTime) continue;
                    regens.Accumulator -= regens.AccumulatorTime;

                    if (regens.solution == null) continue;

                    _solutionSystem.TryAddReagent(autoComp.Owner, regens.solution, regens.reagent, regens.unitsPerUpdate, out var accepted);
                }
            }
        }
    }
}
