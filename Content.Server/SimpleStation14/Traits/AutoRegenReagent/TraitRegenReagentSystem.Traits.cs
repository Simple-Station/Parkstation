namespace Content.Server.SimpleStation14.Traits
{
    public sealed class TraitRegenReagentSystemTraitsSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DraconicBloodstreamComponent, ComponentInit>(OnTraitInit);
            SubscribeLocalEvent<EasyDrunkComponent, ComponentInit>(OnTraitInit);
        }

        private void OnTraitInit(EntityUid uid, AbstractTraitRegenReagentComponent component, ComponentInit args)
        {
            var traitRegen = _entityManager.EnsureComponent<TraitRegenReagentComponent>(uid);
            traitRegen.Reagents.Add(component.Reagent);
            Logger.Info("TraitRegenReagentSystemTraitsSystem: Added {0} to {1}", component.Reagent.reagent, uid.ToString());
        }
    }
}
