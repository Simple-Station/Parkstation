using Content.Server.GameTicking;
using Content.Shared.Preferences;
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

    /// <summary>
    ///     All the jobs that can't have traits.
    /// </summary>
    private readonly List<string> _jobBlacklist = new()
    {
        "SAI",
        "Cyborg",
        "MedicalCyborg"
    };

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        AddTraits(args.Profile, args.JobId, args.Mob);
    }

    /// <summary>
    ///     Adds all traits selected by the player during character creation to the mob.
    /// </summary>
    /// <param name="profile">Character profile, to get the traits</param>
    /// <param name="jobId">Job ID, for job-specific trait checking</param>
    /// <param name="mob">Entity to add the traits to</param>
    /// <returns>Whether or not it succeeded</returns>
    public bool AddTraits(HumanoidCharacterProfile profile, string? jobId, EntityUid mob)
    {
        List<TraitPrototype> traits = new();
        var addTraits = true;

        foreach (var traitId in profile.TraitPreferences)
        {
            // Don't add any traits if the job is blacklisted
            if (jobId != null && _jobBlacklist.Contains(jobId))
            {
                addTraits = false;
                break;
            }

            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Logger.Warning($"No trait found with ID {traitId}!");
                continue;
            }

            if (traitPrototype.Whitelist != null && !traitPrototype.Whitelist.IsValid(mob))
            {
                // You don't get any traits for breaking the whitelist
                if (traitPrototype.Category == "Negative")
                {
                    addTraits = false;
                    break;
                }
                else
                    continue;
            }

            if (traitPrototype.Blacklist != null && traitPrototype.Blacklist.IsValid(mob))
            {
                // You don't get any traits for breaking the blacklist
                if (traitPrototype.Category == "Negative")
                {
                    addTraits = false;
                    break;
                }
                else
                    continue;
            }

            traits.Add(traitPrototype);
        }

        if (!addTraits)
        {
            Logger.Warning($"Not adding traits to entity {mob} because they broke the whitelist/blacklist of a negative trait.");
            return false;
        }

        // Add all components required by the prototypes
        foreach (var trait in traits)
        {
            var entries = trait.Components.Values;
            foreach (var entry in entries)
            {
                var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = mob;
                EntityManager.AddComponent(mob, comp, true);

                // Tell the client to add the trait client-sided too
                RaiseNetworkEvent(new TraitAddedEvent(mob, trait.ID));
            }
        }

        return true;
    }
}
