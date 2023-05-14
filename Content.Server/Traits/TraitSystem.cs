using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.SimpleStation14.Traits.Events;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<BeenClonedEvent>(OnBeenCloned);
    }

    /// <summary>
    ///     When the player is spawned in, add all trait components selected during character creation
    /// </summary>
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        AddTraits(args.Profile, args.JobId, args.Mob);
    }

    /// <summary>
    ///     When the player is cloned, add all trait components selected during character creation
    /// </summary>
    private void OnBeenCloned(BeenClonedEvent args)
    {
        AddTraits(args.Profile, args.Mind.CurrentJob?.Prototype.ID, args.Mob);
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
        List<string> traitString = new();
        var addTraits = true;
        string conflictingTrait = "";
        var blacklistedJob = false;

        foreach (var traitId in profile.TraitPreferences)
        {
            // Don't add any traits if the job is blacklisted
            if (jobId != null && !_prototypeManager.Index<JobPrototype>(jobId).CanHaveTraits)
            {
                blacklistedJob = true;
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
                    conflictingTrait = Loc.GetString(traitPrototype.Name);
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
                    conflictingTrait = Loc.GetString(traitPrototype.Name);
                    break;
                }
                else
                    continue;
            }

            traits.Add(traitPrototype);
        }

        if (!addTraits)
        {
            _popupSystem.PopupEntity(Loc.GetString("trait-blacklist-popup", ("trait", conflictingTrait)), mob, mob, PopupType.LargeCaution);

            Logger.Warning($"Not adding traits to entity {mob} because they broke the whitelist/blacklist of trait {conflictingTrait}.");
        }

        if (blacklistedJob)
        {
            _popupSystem.PopupEntity(Loc.GetString("trait-blacklist-job-popup"), mob, mob, PopupType.LargeCaution);

            Logger.Warning($"Not adding traits to entity {mob} because their job is blacklisted from having traits.");
        }

        if (!addTraits || blacklistedJob)
            return false;

        // Add all components required by the prototypes
        foreach (var trait in traits)
        {
            traitString.Add(trait.ID);

            var entries = trait.Components.Values;
            foreach (var entry in entries)
            {
                var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = mob;
                EntityManager.AddComponent(mob, comp, true);
            }

            // Add item required by the trait
            if (trait.TraitGear != null)
            {
                if (!TryComp(mob, out HandsComponent? handsComponent))
                    continue;

                var coords = EntityManager.GetComponent<TransformComponent>(mob).Coordinates;
                var inhandEntity = EntityManager.SpawnEntity(trait.TraitGear, coords);
                _sharedHandsSystem.TryPickup(mob, inhandEntity, checkActionBlocker: false,
                    handsComp: handsComponent);
            }
        }

        return true;
    }
}
