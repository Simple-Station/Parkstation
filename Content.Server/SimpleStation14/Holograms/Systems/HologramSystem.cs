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

namespace Content.Server.SimpleStation14.Hologram;

public class HologramSystem : EntitySystem
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

    public readonly Dictionary<Mind.Mind, EntityUid> ClonesWaitingForMind = new();

    public override void Initialize()
    {
        base.Initialize();
        // SubscribeLocalEvent<HologramComponent, ComponentStartup>(Startup);
        // SubscribeLocalEvent<HologramComponent, ComponentShutdown>(Shutdown);
        // SubscribeLocalEvent<HoloTeleportEvent>(HoloTeleport);
    }

    // private void Startup(EntityUid uid, HologramComponent component, ComponentStartup args)
    // {
    //     var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
    //     _actionsSystem.AddAction(uid, action, uid);
    // }

    // private void Shutdown(EntityUid uid, HologramComponent component, ComponentShutdown args)
    // {
    //     var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>("ShadekinTeleport"));
    //     _actionsSystem.RemoveAction(uid, action);
    // }


    // Anything that needs to be regularly run, like handling exiting a projector's range
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var component in _entityManager.EntityQuery<HologramComponent>().ToList())
        {
            var projector = HoloGetProjector(component);
            if (projector == EntityUid.Invalid)
            {
                HoloReturn(component);
                continue;
            }
            component.CurProjector = projector;
        }
    }



    /// <summary>
    /// Tests for the nearest projector to the Hologram.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <returns>Returns the UID of the projector, or invalid UID if no projectors are found.</returns>
    private EntityUid HoloGetProjector(HologramComponent component, bool occlude = true, float range = 18f)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var uid = component.Owner;
        var transform = _entityManager.GetComponent<TransformComponent>(uid);
        var playerPos = _transform.GetWorldPosition(transform, xformQuery);
        var mapId = transform.MapID;

        // sort all entities in distance increasing order
        var nearProjList = new SortedList<float, EntityUid>();

        foreach (var comp in _entityManager.EntityQuery<HologramProjectorComponent>())
        {
            if (!xformQuery.TryGetComponent(comp.Owner, out var compXform) || compXform.MapID != mapId)
                continue;

            var dist = (_transform.GetWorldPosition(compXform, xformQuery) - playerPos).LengthSquared;
            nearProjList.TryAdd(dist, comp.Owner);
        }

        foreach (var nearProj in nearProjList)
        {
            if (_entityManager.TryGetComponent<SurveillanceCameraComponent>(nearProj.Value, out var camComp) && !camComp.Active) continue;
            if (occlude && !nearProj.Value.InRangeUnOccluded(uid, 18f)) continue;
            return nearProj.Value;
        }
        return EntityUid.Invalid;
    }

    /// <summary>
    /// Tests for the nearest projector to a set of coords.
    /// </summary>
    /// <param name="coords">Coords to test from.</param>
    /// <param name="mapId">Map being tested on.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <returns>Returns the UID of the projector, or invalid UID if no projectors are found.</returns>
    private EntityUid HoloGetProjector(Vector2 coords, MapId mapId, bool occlude = true, float range = 18f)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();

        // sort all entities in distance increasing order
        var nearProjList = new SortedList<float, EntityUid>();

        foreach (var comp in _entityManager.EntityQuery<HologramProjectorComponent>())
        {
            if (!xformQuery.TryGetComponent(comp.Owner, out var compXform) || compXform.MapID != mapId)
                continue;

            var dist = (_transform.GetWorldPosition(compXform, xformQuery) - coords).LengthSquared;
            nearProjList.TryAdd(dist, comp.Owner);
        }

        foreach (var nearProj in nearProjList)
        {
            if (_entityManager.TryGetComponent<SurveillanceCameraComponent>(nearProj.Value, out var camComp) && !camComp.Active) continue;
            if (occlude && !nearProj.Value.InRangeUnOccluded(new MapCoordinates(coords, mapId), range)) continue;
            return nearProj.Value;
        }
        return EntityUid.Invalid;
    }


    /// <summary>
    ///     Handles returning a Hologram to their last visited projector,
    ///     then to the nearest, finally killing them if none are found.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>
    private void HoloReturn(HologramComponent component)
    {
        var uid = component.Owner;
        var meta = _entityManager.GetComponent<MetaDataComponent>(uid);
        var holoPos = _entityManager.GetComponent<TransformComponent>(uid).Coordinates;

        var popupAppearOther = Loc.GetString("system-hologram-phasing-appear-others", ("name", meta.EntityName));
        var popupAppearSelf = Loc.GetString("system-hologram-phasing-appear-self");
        var popupDisappearOther = Loc.GetString("system-hologram-phasing-disappear-others", ("name", meta.EntityName));
        var popupDeathSelf = Loc.GetString("system-hologram-phasing-death-self");

        if (component.CurProjector == null || !_entityManager.TryGetComponent<TransformComponent>(component.CurProjector, out var _) ||
            (_entityManager.TryGetComponent<SurveillanceCameraComponent>(component.CurProjector, out var camComp) && !camComp.Active))
        {
            component.CurProjector = HoloGetProjector(component, false);
        }

        if (component.CurProjector == EntityUid.Invalid)
        {

            HoloKill(component);
            return;
        }
        _entityManager.TryGetComponent<TransformComponent>(component.CurProjector, out var transfComp);

        Popup.PopupEntity(popupAppearOther, uid, Filter.PvsExcept((EntityUid) uid), false, PopupType.Medium);
        Popup.PopupCoordinates(popupDisappearOther, holoPos, Filter.PvsExcept(uid), false, PopupType.MediumCaution);
        _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(uid), coordinates: holoPos, false);
        if (TryComp<SharedPullableComponent>(uid, out var pullable) && pullable.BeingPulled) _pulling.TryStopPull(pullable);
        if (TryComp<SharedPullerComponent>(uid, out var pulling) && pulling.Pulling != null &&
            TryComp<SharedPullableComponent>(pulling.Pulling.Value, out var subjectPulling)) _pulling.TryStopPull(subjectPulling);
        // Move holo
        Transform(uid).Coordinates = _entityManager.GetComponent<TransformComponent>((EntityUid) component.CurProjector).Coordinates;
        Popup.PopupEntity(popupAppearSelf, uid, uid, PopupType.Large);
        _audio.PlayPvs("/Audio/SimpleStation14/Effects/Hologram/holo_on.ogg", uid);

        _adminLogger.Add(LogType.Unknown, LogImpact.Low,
            $"{ToPrettyString(uid):mob} was returned to projector {ToPrettyString((EntityUid) component.CurProjector):entity}");
    }

    /// <summary>
    ///     Kills a Hologram after playing the visual and auditory effects.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>

    private void HoloKill(HologramComponent component)
    {
        var uid = component.Owner;
        var meta = _entityManager.GetComponent<MetaDataComponent>(uid);
        var holoPos = _entityManager.GetComponent<TransformComponent>(uid).Coordinates;
        EntityUid? body = EntityUid.Invalid;
        Mind.Mind? mind = null;

        var popupAppearOther = Loc.GetString("system-hologram-phasing-appear-others", ("name", meta.EntityName));
        var popupAppearSelf = Loc.GetString("system-hologram-phasing-appear-self");
        var popupDisappearOther = Loc.GetString("system-hologram-phasing-disappear-others", ("name", meta.EntityName));
        var popupDeathSelf = Loc.GetString("system-hologram-phasing-death-self");

        if (_entityManager.TryGetComponent<MindComponent>(uid, out var mindComp) && mindComp.Mind != null)
        {
            body = mindComp.Mind.OwnedEntity;
            mind = mindComp.Mind;
            EntitySystem.Get<GameTicker>().OnGhostAttempt(mindComp.Mind, false);
        }

        _audio.Play(filename: "/Audio/SimpleStation14/Effects/Hologram/holo_off.ogg", playerFilter: Filter.Pvs(uid), coordinates: holoPos, false);
        Popup.PopupCoordinates(popupDisappearOther, holoPos, Filter.PvsExcept(uid), false, PopupType.MediumCaution);
        Popup.PopupCoordinates(popupDeathSelf, holoPos, uid, PopupType.LargeCaution);
        _entityManager.DeleteEntity(uid);

        _adminLogger.Add(LogType.Unknown, LogImpact.Medium, $"{ToPrettyString(uid):mob} was disabled due to lack of projectors");

        // var holopodQuery = _entityManager.EntityQuery<HolopodComponent>();
        // while (true)
        // {
        //     Logger.Info("Check");
        //     foreach (var holopod in holopodQuery)
        //     {
        //         if (mind != null && TryHoloGenerate(holopod.Owner, mind, _entityManager.GetComponent<CloningPodComponent>(holopod.Owner)))
        //         {
        //             Logger.Warning("They got cloned!");
        //             return;
        //         }
        //     }
        // }
    }


    // public bool TryHoloGenerate(EntityUid uid, Mind.Mind mind, CloningPodComponent? clonePod)
    // {
    //     CloningSystem cloneSys = new();
    //     Logger.Info("Trying to clone");

    //     if (!Resolve(uid, ref clonePod))
    //         return false;

    //     if (HasComp<ActiveCloningPodComponent>(uid))
    //         return false;

    //     Logger.Info("Clone pod is active");

    //     if (ClonesWaitingForMind.TryGetValue(mind, out var clone))
    //     {
    //         if (EntityManager.EntityExists(clone) &&
    //             !_mobStateSystem.IsDead(clone) &&
    //             TryComp<MindComponent>(clone, out var cloneMindComp) &&
    //             (cloneMindComp.Mind == null || cloneMindComp.Mind == mind))
    //             return false; // Mind already has clone

    //         ClonesWaitingForMind.Remove(mind);
    //     }
    //     Logger.Info("Waiting something something");

    //     if (mind.OwnedEntity != null && !_mobStateSystem.IsDead(mind.OwnedEntity.Value))
    //         return false; // Body controlled by mind is not dead
    //     Logger.Info("Not alive still");

    //     // Yes, we still need to track down the client because we need to open the Eui
    //     if (mind.UserId == null || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client))
    //         return false; // If we can't track down the client, we can't offer transfer. That'd be quite bad.
    //     Logger.Warning("Got client");

    //     var pref = (HumanoidCharacterProfile) _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter;

    //     if (pref == null)
    //         return false;
    //     Logger.Warning("Got prefs");

    //     var mob = HoloFetchAndSpawn(clonePod, pref);

    //     var cloneMindReturn = EntityManager.AddComponent<BeingClonedComponent>(mob);
    //     cloneMindReturn.Mind = mind;
    //     cloneMindReturn.Parent = clonePod.Owner;
    //     clonePod.BodyContainer.Insert(mob);
    //     ClonesWaitingForMind.Add(mind, mob);
    //     _cloningSystem.UpdateStatus(CloningPodStatus.NoMind, clonePod);
    //     _euiManager.OpenEui(new AcceptCloningEui(mind, cloneSys), client);

    //     AddComp<ActiveCloningPodComponent>(uid);

    //     // TODO: Ideally, components like this should be on a mind entity so this isn't neccesary.
    //     // Remove this when 'mind entities' are added.
    //     // Add on special job components to the mob.
    //     if (mind.CurrentJob != null)
    //     {
    //         foreach (var special in mind.CurrentJob.Prototype.Special)
    //         {
    //             if (special is AddComponentSpecial)
    //                 special.AfterEquip(mob);
    //         }
    //     }

    //     return true;
    // }


    // /// <summary>
    // /// Handles fetching the mob and any appearance stuff...
    // /// </summary>
    // private EntityUid HoloFetchAndSpawn(CloningPodComponent clonePod, HumanoidCharacterProfile pref)
    // {
    //     List<Sex> sexes = new();
    //     var name = pref.Name;
    //     var toSpawn = "MobHologram";

    //     var mob = Spawn(toSpawn, Transform(clonePod.Owner).MapPosition);
    //     _humanoidSystem.LoadProfile(mob, pref);

    //     MetaData(mob).EntityName = name;
    //     var mind = EnsureComp<MindComponent>(mob);
    //     _mind.SetExamineInfo(mob, true, mind);

    //     var grammar = EnsureComp<GrammarComponent>(mob);
    //     grammar.ProperNoun = true;
    //     grammar.Gender = Robust.Shared.Enums.Gender.Neuter;
    //     Dirty(grammar);

    //     RemComp<PotentialPsionicComponent>(mob);
    //     EnsureComp<SpeechComponent>(mob);
    //     EnsureComp<EmotingComponent>(mob);
    //     RemComp<ReplacementAccentComponent>(mob);
    //     RemComp<MonkeyAccentComponent>(mob);
    //     RemComp<SentienceTargetComponent>(mob);
    //     RemComp<GhostTakeoverAvailableComponent>(mob);

    //     _tag.AddTag(mob, "DoorBumpOpener");

    //     return mob;
    // }




    // private void HoloTeleport(HoloTeleportEvent args)
    // {
    // if (args.Handled) return;

    // if HoloGetProjector(args.Target, args. )
    // var transform = Transform(args.Performer);
    // if (transform.MapID != args.Target.GetMapId(EntityManager)) return;

    // _transformSystem.SetCoordinates(args.Performer, args.Target);
    // transform.AttachToGridOrMap();

    // _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));

    // _staminaSystem.TakeStaminaDamage(args.Performer, 35);

    // args.Handled = true;
    // }

    public struct HoloDataEntry {
        public Mind.Mind Mind;
        public HumanoidCharacterProfile Profile;

        public HoloDataEntry(Mind.Mind m, HumanoidCharacterProfile hcp)
        {
            Mind = m;
            Profile = hcp;
        }
    }
}
