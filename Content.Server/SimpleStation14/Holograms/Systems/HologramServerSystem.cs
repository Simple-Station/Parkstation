using Content.Server.SurveillanceCamera;
using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Content.Shared.Interaction.Helpers;
using Content.Shared.SimpleStation14.Hologram;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Cloning;
using Content.Server.Cloning.Components;

using Content.Shared.Cloning;
using Content.Shared.Speech;
using Content.Shared.Preferences;
using Content.Shared.Emoting;
using Content.Server.Psionics;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Server.EUI;
using Content.Server.Humanoid;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Jobs;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects.Components.Localization;
using System.Linq;
using Robust.Shared.Containers;
using Content.Shared.Interaction;

namespace Content.Server.SimpleStation14.Hologram;

public class HologramServerSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
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

    private const string DiskSlot = "holo_disk";
    public readonly Dictionary<Mind.Mind, EntityUid> ClonesWaitingForMind = new();

    public override void Initialize()
    {
        base.Initialize();
        // SubscribeLocalEvent<HologramServerComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<HologramDiskComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeNetworkEvent<HologramDiskInsertedEvent>(TryHoloGenerate);
    }

    public void TryHoloGenerate(HologramDiskInsertedEvent args)
    {
        var uid = args.ServerComponent.Owner;
        var clonePod = _entityManager.GetComponent<CloningPodComponent>(uid);
        var disk = _entityManager.GetComponent<HologramDiskComponent>(args.Uid);
        var mind = disk.HoloData!;

        CloningSystem cloneSys = new();
        Logger.Info("Trying to clone");

        if (!Resolve(uid, ref clonePod))
            return;

        if (HasComp<ActiveCloningPodComponent>(uid))
            return;

        Logger.Info("Clone pod is active");

        if (ClonesWaitingForMind.TryGetValue(mind, out var clone))
        {
            if (EntityManager.EntityExists(clone) &&
                !_mobStateSystem.IsDead(clone) &&
                TryComp<MindComponent>(clone, out var cloneMindComp) &&
                (cloneMindComp.Mind == null || cloneMindComp.Mind == mind))
                return; // Mind already has clone

            ClonesWaitingForMind.Remove(mind);
        }
        Logger.Info("Waiting something something");

        if (mind.OwnedEntity != null && !_mobStateSystem.IsDead(mind.OwnedEntity.Value))
            return; // Body controlled by mind is not dead
        Logger.Info("Not alive still");

        // Yes, we still need to track down the client because we need to open the Eui
        if (mind.UserId == null || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client))
            return; // If we can't track down the client, we can't offer transfer. That'd be quite bad.
        Logger.Info("Got client");

        var pref = (HumanoidCharacterProfile) _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter;

        if (pref == null)
            return;
        Logger.Info("Got prefs");

        var mob = HoloFetchAndSpawn(clonePod, pref);

        var cloneMindReturn = EntityManager.AddComponent<BeingClonedComponent>(mob);
        cloneMindReturn.Mind = mind;
        cloneMindReturn.Parent = clonePod.Owner;
        // clonePod.BodyContainer.Insert(mob);
        ClonesWaitingForMind.Add(mind, mob);
        UpdateStatus(CloningPodStatus.NoMind, clonePod);
        _euiManager.OpenEui(new AcceptCloningEui(mind, cloneSys), client);

        Logger.Warning("Cloned");

        AddComp<ActiveCloningPodComponent>(uid);

        // TODO: Ideally, components like this should be on a mind entity so this isn't neccesary.
        // Remove this when 'mind entities' are added.
        // Add on special job components to the mob.
        if (mind.CurrentJob != null)
        {
            foreach (var special in mind.CurrentJob.Prototype.Special)
            {
                if (special is AddComponentSpecial)
                    special.AfterEquip(mob);
            }
        }

        return;
    }

    public void UpdateStatus(CloningPodStatus status, CloningPodComponent cloningPod)
    {
        cloningPod.Status = status;
    }

    /// <summary>
    /// Handles fetching the mob and any appearance stuff...
    /// </summary>
    private EntityUid HoloFetchAndSpawn(CloningPodComponent clonePod, HumanoidCharacterProfile pref)
    {
        List<Sex> sexes = new();
        var name = pref.Name;
        var toSpawn = "MobHologram";

        var mob = Spawn(toSpawn, Transform(clonePod.Owner).MapPosition);
        _humanoidSystem.LoadProfile(mob, pref);

        MetaData(mob).EntityName = name;
        var mind = EnsureComp<MindComponent>(mob);
        _mind.SetExamineInfo(mob, true, mind);

        var grammar = EnsureComp<GrammarComponent>(mob);
        grammar.ProperNoun = true;
        grammar.Gender = Robust.Shared.Enums.Gender.Neuter;
        Dirty(grammar);

        RemComp<PotentialPsionicComponent>(mob);
        EnsureComp<SpeechComponent>(mob);
        EnsureComp<EmotingComponent>(mob);
        RemComp<ReplacementAccentComponent>(mob);
        RemComp<MonkeyAccentComponent>(mob);
        RemComp<SentienceTargetComponent>(mob);
        RemComp<GhostTakeoverAvailableComponent>(mob);

        _tag.AddTag(mob, "DoorBumpOpener");

        return mob;
    }


    private void OnAfterInteract(EntityUid uid, HologramDiskComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !TryComp<MindComponent>(args.Target, out var targetMind) || targetMind.Mind == null)
            return;

        component.HoloData = targetMind.Mind;
        Popup.PopupEntity(Loc.GetString("Data saved, boi"), args.Target.Value, args.User);
    }

}
