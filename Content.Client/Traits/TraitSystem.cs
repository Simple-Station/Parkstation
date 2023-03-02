using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Client.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<TraitAddedEvent>(OnTraitAdded);
    }

    private void OnTraitAdded(TraitAddedEvent args)
    {
        if (!_prototypeManager.TryIndex<TraitPrototype>(args.Trait, out var traitPrototype))
        {
            Logger.Warning($"No trait found with ID {args.Trait}!");
            return;
        }

        // Add all components required by the prototype
        foreach (var entry in traitPrototype.Components.Values)
        {
            var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = args.Uid;
            EntityManager.AddComponent(args.Uid, comp, true);
        }
    }
}
