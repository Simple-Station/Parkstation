using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Player;
using Content.Server.Cloning.Components;
using Content.Server.Psionics;
using Content.Server.Humanoid;
using Content.Server.Jobs;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Power.Components;
using Content.Server.Administration.Commands;
using Content.Shared.Tag;
using Content.Shared.Speech;
using Content.Shared.Preferences;
using Content.Shared.Emoting;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Access.Components;
using Content.Shared.Clothing.Components;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects.Components.Localization;
using Content.Shared.Movement.Systems;
using System.Threading.Tasks;
using Content.Shared.SimpleStation14.Holograms.Components;
using Content.Server.SimpleStation14.Holograms.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Server.EUI;
using Robust.Server.GameObjects;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Content.Server.Access.Systems;
using Content.Server.Station.Systems;
using Content.Server.Station.Components;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class HologramSystem : SharedHologramSystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPlayerManager _playerManager = null!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public readonly Dictionary<Mind.Mind, EntityUid> HologramsWaitingForMind = new();
    /// <summary>
    ///     Handles killing a Hologram, with no checks in place.
    /// </summary>
    /// <remarks>
    ///     You should generally use <see cref="SharedHologramSystem.TryKillHologram"/> instead.
    /// </remarks>
    public override void DoKillHologram(EntityUid hologram, HologramComponent? holoComp = null) // TODOPark: HOLO Move this to Shared once Upstream merge.
    {
        if (!Resolve(hologram, ref holoComp))
            return;

        var meta = MetaData(hologram);
        var holoPos = Transform(hologram).Coordinates;

        if (TryComp<MindContainerComponent>(hologram, out var mindComp) && mindComp.Mind != null)
            _gameTicker.OnGhostAttempt(mindComp.Mind, false);

        _audio.Play(holoComp.OffSound, playerFilter: Filter.Pvs(hologram), coordinates: holoPos, false);
        _popup.PopupCoordinates(Loc.GetString(holoComp.PopupDisappearOther, ("name", meta.EntityName)), holoPos, Filter.PvsExcept(hologram), false, PopupType.MediumCaution);
        _popup.PopupCoordinates(Loc.GetString(holoComp.PopupDeathSelf), holoPos, hologram, PopupType.LargeCaution);

        _entityManager.QueueDeleteEntity(hologram);

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(hologram):mob} was killed!");
    }

    public bool TryGenerateHologram(Mind.Mind mind, EntityCoordinates coords, [NotNullWhen(true)] out EntityUid? holo)
    {
        holo = null;

        if (HologramsWaitingForMind.TryGetValue(mind, out var clone))
        {
            if (EntityManager.EntityExists(clone) &&
                !_mobState.IsDead(clone) &&
                TryComp<MindContainerComponent>(clone, out var cloneMindComp) &&
                (cloneMindComp.Mind == null || cloneMindComp.Mind == mind))
                return false; // Mind already has clone

            HologramsWaitingForMind.Remove(mind);
        }

        if (mind.OwnedEntity != null && (_mobState.IsAlive(mind.OwnedEntity.Value) || _mobState.IsCritical(mind.OwnedEntity.Value)))
            return false; // Body controlled by mind is not dead

        // Yes, we still need to track down the client because we need to open the Eui
        if (mind.UserId == null || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client))
            return false; // If we can't track down the client, we can't offer transfer. That'd be quite bad.

        var pref = (HumanoidCharacterProfile) _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter;

        var mob = HoloFetchAndSpawn(pref, coords, "MobHologramProjected");

        HologramsWaitingForMind.Add(mind, mob);
        _eui.OpenEui(new AcceptHologramEui(mind, this), client);

        if (mind.CurrentJob != null)
        {
            foreach (var special in mind.CurrentJob.Prototype.Special)
                if (special is AddComponentSpecial)
                    special.AfterEquip(mob);

            // Get each access from the job prototype and add it to the mob
            var extended = _station.GetOwningStation(mob) is { } station && TryComp<StationJobsComponent>(station, out var jobComp) && jobComp.ExtendedAccess;
            _access.SetAccessToJob(mob, mind.CurrentJob.Prototype, extended, EnsureComp<AccessComponent>(mob));

            // Get the loadout from the job prototype and add it to the Hologram making each item unremovable.
            if (mind.CurrentJob.Prototype.StartingGear != null)
            {
                SetOutfitCommand.SetOutfit(mob, mind.CurrentJob.Prototype.StartingGear, EntityManager, (_, item) =>
                {
                    if (TryComp<ClothingComponent>(item, out var clothing))
                    {
                        if (clothing.InSlot is "back" or "pocket1" or "pocket2" or "belt" or "suitstorage" or "id")
                        {
                            QueueDel(item);
                            return;
                        }
                    }
                    EnsureComp<HologramComponent>(item);
                    EnsureComp<UnremoveableComponent>(item);
                });
            }
        }

        _adminLogger.Add(LogType.Mind, LogImpact.Medium,
            $"Hologram {ToPrettyString(mob):mob} was generated at {coords}");

        holo = mob;
        return true;
    }

    internal void TransferMindToHologram(Mind.Mind mind)
    {
        if (!HologramsWaitingForMind.TryGetValue(mind, out var entity) ||
            !EntityManager.EntityExists(entity) ||
            !TryComp<MindContainerComponent>(entity, out var mindComp) ||
            mindComp.Mind != null)
            return;

        _mind.TransferTo(mind, entity, true);
        _mind.UnVisit(mind);

        HologramsWaitingForMind.Remove(mind);
    }

    /// <summary>
    ///     Handles fetching the mob and any appearance stuff...
    /// </summary>
    private EntityUid HoloFetchAndSpawn(HumanoidCharacterProfile pref, EntityCoordinates coords, string mobPrototype)
    {
        var mob = Spawn(mobPrototype, coords);
        _transform.AttachToGridOrMap(mob);

        _humanoid.LoadProfile(mob, pref);
        _meta.SetEntityName(mob, pref.Name);

        var mind = EnsureComp<MindContainerComponent>(mob);
        _mind.SetExamineInfo(mob, true, mind);

        var grammar = EnsureComp<GrammarComponent>(mob);
        grammar.ProperNoun = true;
        grammar.Gender = Gender.Neuter;
        Dirty(grammar);

        return mob;
    }
}
