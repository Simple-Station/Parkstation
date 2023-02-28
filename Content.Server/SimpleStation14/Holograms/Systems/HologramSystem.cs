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
using Robust.Shared.Utility;

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
        SubscribeAllEvent<HologramKillEvent>(HoloKill);
        SubscribeAllEvent<HologramGetProjectorEvent>(HoloGetProjector);
        SubscribeAllEvent<HologramProjectorTestEvent>(HoloProjectorTest);
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
            var nearProj = HoloGetProjector(component);
            if (!nearProj.IsValid())
            {
                if (component.Accumulator > 0)
                {
                    component.Accumulator -= frameTime;
                    continue;
                }
                RaiseLocalEvent(new HologramReturnEvent(component.Owner));
                continue;
            }
            component.Accumulator = 0.24f;
            component.CurProjector = nearProj;
        }
    }

    /// <summary>
    /// Tests if the given projector is valid.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    public void HoloProjectorTest(HologramProjectorTestEvent args)
    {
        var curProjector = args.Projector;
        if (curProjector == EntityUid.Invalid || !_entityManager.TryGetComponent<TransformComponent>(curProjector, out var _)) return;
        if (_entityManager.TryGetComponent<SurveillanceCameraComponent>(curProjector, out var camComp) && !camComp.Active) return;
        args.CanProject = true;
    }

    /// <summary>
    /// Tests for the nearest projector to the Hologram.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    public void HoloGetProjector(HologramGetProjectorEvent args)
    {
        var uid = args.Hologram;
        var component = _entityManager.GetComponent<HologramComponent>(uid);
        var occlude = args.Occlude;
        var range = args.Range;

        var xformQuery = GetEntityQuery<TransformComponent>();
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
            args.Projector = nearProj.Value;
            return;
        }
        return;
    }

    /// <summary>
    /// Tests for the nearest projector to the Hologram.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>
    /// <param name="occlude">Should it check only for unoccluded and in range projectors?</param>
    /// <param name="range">The range it should check for projectors in, if occlude is true</param>
    /// <returns>Returns the UID of the projector, or invalid UID if no projectors are found.</returns>
    public EntityUid HoloGetProjector(HologramComponent component, bool occlude = true, float range = 18f)
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
    public EntityUid HoloGetProjector(Vector2 coords, MapId mapId, bool occlude = true, float range = 18f)
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
    ///     Kills a Hologram after playing the visual and auditory effects.
    /// </summary>
    /// <param name="component">Hologram's HologramComponent.</param>
    public void HoloKill(HologramKillEvent args)
    {
        var uid = args.Uid;
        var component = _entityManager.GetComponent<HologramComponent>(uid);
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
        if (component.LinkedServer != EntityUid.Invalid)
        {
            if (_entityManager.TryGetComponent<HologramServerComponent>(component.LinkedServer!.Value, out var serverComp))
                serverComp.LinkedHologram = EntityUid.Invalid;
            component.LinkedServer = EntityUid.Invalid;
        }

        _entityManager.DeleteEntity(uid);

        _adminLogger.Add(LogType.Unknown, LogImpact.Medium, $"{ToPrettyString(uid):mob} was disabled due to lack of projectors");
    }

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
    // }
}
