using Content.Server.GameTicking;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private readonly List<string> _jobBlacklist = new()
    {
        "SAI",
        "Cyborg",
        "MedicalCyborg"
    };

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        List<TraitPrototype> traits = new();
        var addTraits = true;

        foreach (var traitId in args.Profile.TraitPreferences)
        {
            // Don't add any traits if the job is blacklisted
            if (args.JobId != null && _jobBlacklist.Contains(args.JobId))
            {
                addTraits = false;
                break;
            }

            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Logger.Warning($"No trait found with ID {traitId}!");
                continue;
            }

            if (traitPrototype.Whitelist != null && !traitPrototype.Whitelist.IsValid(args.Mob))
            {
                // You don't get any traits for breaking the whitelist
                if (traitPrototype.Category == "Negative")
                {
                    addTraits = false;
                    break;
                }
                else continue;
            }

            if (traitPrototype.Blacklist != null && traitPrototype.Blacklist.IsValid(args.Mob))
            {
                // You don't get any traits for breaking the blacklist
                if (traitPrototype.Category == "Negative")
                {
                    addTraits = false;
                    break;
                }
                else continue;
            }

            traits.Add(traitPrototype);
        }

        if (!addTraits)
        {
            Logger.Warning($"Not adding traits to {args.Player.Name} because they broke the whitelist/blacklist of a negative trait.");
            return;
        }

        // Add all components required by the prototypes
        foreach (var trait in traits)
        {
            var entries = trait.Components.Values;
            foreach (var entry in entries)
            {
                var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = args.Mob;
                EntityManager.AddComponent(args.Mob, comp, true);

                // Tell the client to add the trait clientsided too
                RaiseNetworkEvent(new TraitAddedEvent(args.Mob, trait.ID));
            }
        }
    }
}
