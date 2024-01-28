using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Construction.Steps;
using System.Collections.Generic;
using Content.Shared.Tag;

namespace Content.Shared.SimpleStation14.Construction.Steps
{
    [DataDefinition]
    public sealed class FilterConstructionGraphStep : ArbitraryInsertConstructionGraphStep
    {
        [DataField("size")] public int Size { get; }
        [DataField("compBlacklist")] public List<string> CompBlacklist { get; } = new List<string>();
        [DataField("tagBlacklist")] public List<string> TagBlacklist { get; } = new List<string>();

        public override bool EntityValid(EntityUid uid, IEntityManager entityManager, IComponentFactory compFactory)
        {
            if (!entityManager.TryGetComponent<ItemComponent>(uid, out var item) || item.Size > Size)
                return false;

            foreach (var component in CompBlacklist)
            {
                if (entityManager.HasComponent(uid, compFactory.GetComponent(component).GetType()))
                    return false;
            }

            var tagSystem = entityManager.EntitySysManager.GetEntitySystem<TagSystem>();

            if (tagSystem.HasAnyTag(uid, TagBlacklist))
                return false;

            return true;
        }

        public override void DoExamine(ExaminedEvent examinedEvent)
        {
            examinedEvent.Message.AddMarkup(string.IsNullOrEmpty(Name)
                ? Loc.GetString(
                    "construction-insert-entity-below-size",
                    ("size", Size))
                : Loc.GetString(
                    "construction-insert-exact-entity",
                    ("entityName", Name)));
        }
    }
}
