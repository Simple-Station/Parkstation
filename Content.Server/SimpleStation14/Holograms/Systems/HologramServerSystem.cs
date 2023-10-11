using System.Linq;
using Content.Server.Cloning;
using Content.Server.Cloning.Components;
using Content.Server.Psionics;
using Content.Server.EUI;
using Content.Server.Humanoid;
using Content.Server.Jobs;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Power.Components;
using Content.Server.Administration.Commands;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.Pulling;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Speech;
using Content.Shared.Preferences;
using Content.Shared.Emoting;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Interaction.Components;
using Content.Shared.Access.Components;
using Content.Shared.Clothing.Components;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects.Components.Localization;
using Content.Shared.Movement.Systems;
using System.Threading.Tasks;
using Content.Server.Mind.Components;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class HologramServerSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPlayerManager _playerManager = null!;
    [Dependency] private readonly CloningSystem _cloningSystem = default!;
    [Dependency] private readonly EuiManager _euiManager = null!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly HologramSystem _hologram = default!;

    private const string DiskSlot = "holo_disk";
    public readonly Dictionary<Mind.Mind, EntityUid> ClonesWaitingForMind = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HologramServerComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<HologramServerComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<HologramDiskComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HologramServerComponent, PowerChangedEvent>(OnPowerChanged);
    }

    /// <summary>
    /// Handles generating a hologram from an inserted disk
    /// </summary>
    private void OnEntInserted(EntityUid uid, HologramServerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != DiskSlot || !_tagSystem.HasTag(args.Entity, "HoloDisk") ||
            _entityManager.TryGetComponent<HologramDiskComponent>(args.Entity, out var diskComp) && diskComp.HoloMind == null) return;

        if (component.LinkedHologram != EntityUid.Invalid && _entityManager.EntityExists(component.LinkedHologram))
        {
            _hologram.DoKillHologram(component.LinkedHologram.Value);
        }

        if (TryHoloGenerate(uid, _entityManager.GetComponent<HologramDiskComponent>(args.Entity).HoloMind!, component, out var holo))
        {
            var holoComp = _entityManager.GetComponent<HologramComponent>(holo);
            component.LinkedHologram = holo;
            holoComp.LinkedServer = uid;
        }
    }

    /// <summary>
    /// Handles killing a hologram when a disk is removed
    /// </summary>
    private void OnEntRemoved(EntityUid uid, HologramServerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != DiskSlot || !_tagSystem.HasTag(args.Entity, "HoloDisk") ||
            (_entityManager.TryGetComponent<HologramDiskComponent>(args.Entity, out var diskComp) && diskComp.HoloMind == null)) return;

        if (_entityManager.EntityExists(component.LinkedHologram))
        {
            _hologram.DoKillHologram(component.LinkedHologram.Value);
        }
    }

    /// <summary>
    /// Called when the server's power state changes
    /// </summary>
    /// <param name="uid">The entity uid of the server</param>
    /// <param name="component">The HologramServerComponent</param>
    /// <param name="args">The PowerChangedEvent</param>
    private void OnPowerChanged(EntityUid uid, HologramServerComponent component, ref PowerChangedEvent args)
    {
        // If the server is no longer powered
        if (!args.Powered && component.LinkedHologram != null && component.LinkedHologram != EntityUid.Invalid)
        {
            // If the hologram exists
            if (_entityManager.EntityExists(component.LinkedHologram))
            {
                // Kill the Hologram
                _hologram.DoKillHologram(component.LinkedHologram.Value);
            }
        }
        // If the server is powered
        else if (args.Powered && component.LinkedHologram == EntityUid.Invalid)
        {
            var serverContainer = _entityManager.GetComponent<ContainerManagerComponent>(component.Owner);
            if (serverContainer.GetContainer(DiskSlot).ContainedEntities.Count <= 0)
            {
                return; // No disk in the server
            }
            var disk = serverContainer.GetContainer(DiskSlot).ContainedEntities.First();
            var diskData = _entityManager.GetComponent<HologramDiskComponent>(disk).HoloMind;

            // If the hologram is generated successfully
            if (diskData != null && TryHoloGenerate(component.Owner, diskData, component, out var holo))
            {
                // Set the linked hologram to the generated hologram
                var holoComp = _entityManager.GetComponent<HologramComponent>(holo);
                component.LinkedHologram = holo;
                holoComp.LinkedServer = component.Owner;
            }
        }
    }

    public bool TryHoloGenerate(EntityUid uid, Mind.Mind mind, HologramServerComponent? holoServer, out EntityUid holo)
    {
        CloningSystem cloneSys = new();
        holo = EntityUid.Invalid;

        if (ClonesWaitingForMind.TryGetValue(mind, out var clone))
        {
            if (EntityManager.EntityExists(clone) &&
                !_mobStateSystem.IsDead(clone) &&
                TryComp<MindContainerComponent>(clone, out var cloneMindComp) &&
                (cloneMindComp.Mind == null || cloneMindComp.Mind == mind))
                return false; // Mind already has clone

            ClonesWaitingForMind.Remove(mind);
        }

        if (mind.OwnedEntity != null && (_mobStateSystem.IsAlive(mind.OwnedEntity.Value) || _mobStateSystem.IsCritical(mind.OwnedEntity.Value)))
            return false; // Body controlled by mind is not dead

        // Yes, we still need to track down the client because we need to open the Eui
        if (mind.UserId == null || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client))
            return false; // If we can't track down the client, we can't offer transfer. That'd be quite bad.

        var pref = (HumanoidCharacterProfile) _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter;

        var mob = HoloFetchAndSpawn(holoServer!, pref);

        var cloneMindReturn = EntityManager.AddComponent<BeingClonedComponent>(mob);
        cloneMindReturn.Mind = mind;
        ClonesWaitingForMind.Add(mind, mob);
        TransferMindToClone(mind);

        if (mind.CurrentJob != null)
        {
            foreach (var special in mind.CurrentJob.Prototype.Special)
            {
                if (special is AddComponentSpecial)
                    special.AfterEquip(mob);
            }

            // Get each access from the job prototype and add it to the mob
            foreach (var access in mind.CurrentJob.Prototype.Access)
            {
                var accessComp = EntityManager.EnsureComponent<AccessComponent>(mob);
                accessComp.Tags.Add(access);
            }

            // Get the loadout from the job prototype and add it to the Hologram
            // making each item unremovable and hardlight.
            if (mind.CurrentJob.Prototype.StartingGear != null)
            {
                SetOutfitCommand.SetOutfit(mob, mind.CurrentJob.Prototype.StartingGear, _entityManager, (_, item) =>
                {
                    if (_entityManager.TryGetComponent<ClothingComponent>(item, out var clothing))
                    {
                        if (clothing.InSlot is "back" or "pocket1" or "pocket2" or "belt" or "suitstorage" or "id")
                        {
                            _entityManager.DeleteEntity(item);
                            return;
                        }
                    }
                    _tagSystem.AddTag(item, "Softlight");
                    _entityManager.EnsureComponent<UnremoveableComponent>(item);
                });
                // HoloEquip(mob, mind.CurrentJob.Prototype);
            }
        }

        _adminLogger.Add(LogType.Unknown, LogImpact.Medium,
            $"{ToPrettyString(mob):mob} was generated at {ToPrettyString((EntityUid) uid):entity}");

        holo = mob;
        return true;
    }

    internal void TransferMindToClone(Mind.Mind mind)
    {
        if (!ClonesWaitingForMind.TryGetValue(mind, out var entity) ||
            !EntityManager.EntityExists(entity) ||
            !TryComp<MindContainerComponent>(entity, out var mindComp) ||
            mindComp.Mind != null)
            return;

        _mind.TransferTo(mind, entity, true);
        _mind.UnVisit(mind);

        ClonesWaitingForMind.Remove(mind);
    }

    /// <summary>
    /// Handles fetching the mob and any appearance stuff...
    /// </summary>
    private EntityUid HoloFetchAndSpawn(HologramServerComponent holoServer, HumanoidCharacterProfile pref)
    {
        List<Sex> sexes = new();
        var name = pref.Name;
        var toSpawn = "MobHologramProjected";

        var mob = Spawn(toSpawn, Transform(holoServer.Owner).MapPosition);
        Transform(mob).AttachToGridOrMap();

        ResetCamera(mob);

        _humanoidSystem.LoadProfile(mob, pref);

        MetaData(mob).EntityName = name;
        var mind = EnsureComp<MindContainerComponent>(mob);
        _mind.SetExamineInfo(mob, true, mind);

        var grammar = EnsureComp<GrammarComponent>(mob);
        grammar.ProperNoun = true;
        grammar.Gender = Robust.Shared.Enums.Gender.Neuter;
        Dirty(grammar);

        var popupAppearOther = Loc.GetString("system-hologram-phasing-appear-others", ("name", MetaData(mob).EntityName));
        var popupAppearSelf = Loc.GetString("system-hologram-phasing-appear-self");

        _popup.PopupEntity(popupAppearOther, mob, Filter.PvsExcept(mob), false, PopupType.Medium);
        _popup.PopupEntity(popupAppearSelf, mob, mob, PopupType.Large);
        _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", mob);

        EnsureComp<SpeechComponent>(mob);
        EnsureComp<EmotingComponent>(mob);
        EnsureComp<HologramComponent>(mob);
        RemComp<PotentialPsionicComponent>(mob);

        _tag.AddTag(mob, "DoorBumpOpener");

        return mob;
    }

    private async void ResetCamera(EntityUid mob)
    {
        await Task.Delay(500);

        _mover.ResetCamera(mob);
    }


    /// <summary>
    /// WiP for equipping unique items based on job.
    /// </summary>
//     private void HoloEquip(EntityUid mob, JobPrototype job)
//     {
//         // Check what job they are against a list, and output a hardcoded item for each.




//         var mobTransform = EntityManager.GetComponent<TransformComponent>(mob);
//         var mobPos = mobTransform.WorldPosition;

//         if (!_entityManager.TryGetComponent<InventoryComponent>(mob, out var mobInv))
//             return;
//         var mobHands = EntityManager.EnsureComponent<HandsComponent>(mob);

//         foreach (var item in job.StartingGear.Items)
//         {
//             var itemEnt = Spawn(item.Prototype, mobPos);
//             _entityManager.GetComponent<TransformComponent>(itemEnt).AttachToGridOrMap();

//             if (item.Slot == null)
//             {
//                 mobInv.TryPutInHandOrAny(itemEnt, out var _);
//             }
//             else
//             {
//                 mobInv.TryPutInSlot(itemEnt, item.Slot);
//             }
//         }
//     }

    private void OnAfterInteract(EntityUid uid, HologramDiskComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !TryComp<MindContainerComponent>(args.Target, out var targetMind))
            return;

        if (targetMind.Mind == null)
        {
            _popup.PopupEntity(Loc.GetString("system-hologram-disk-mind-none"), args.Target.Value, args.User);
            args.Handled = true;

            return;
        }

        component.HoloMind = targetMind.Mind;
        _popup.PopupEntity(Loc.GetString("system-hologram-disk-mind-saved"), args.Target.Value, args.User);

        args.Handled = true;
    }


//     // List of jobs with hardcoded items for Holograms, like a Clown's horn.
//     static readonly Dictionary<string, string> HoloJobItems = new Dictionary<string, string>
//     {
//         { "Clown", "BikeHorn" },
//         { "value2", "output2" },
//         { "value3", "output3" }
//     };
}
