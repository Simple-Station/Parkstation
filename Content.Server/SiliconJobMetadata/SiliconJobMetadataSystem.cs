using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Robust.Shared.Random;
using Content.Server.Station.Systems;

namespace Content.Server.SiliconJobMetadata;

public sealed class SiliconJobMetadataSystem : EntitySystem
{
    // [Dependency] private readonly IPrototypeManager _prototype = default!;
    // [Dependency] private readonly IRobustRandom _random = default!;
    // [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<PlayerSpawningEvent>(OnSpawnPlayer);
        // SubscribeLocalEvent<SiliconJobMetadataComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PlayerMobSpawnedEvent>(OnSpawnPlayer);
    }

    // This is done on map init so that map-placed entities have it randomized each time the map loads, for fun.
    private void OnSpawnPlayer(PlayerMobSpawnedEvent args)
    {
        Logger.Debug("SiliconJobMetadataSystem.OnSpawnPlayer");
        var mob = args.SpawnResult;
        var meta = MetaData(mob);

        if (args.Profile != null)
        {
            // meta.EntityName = args.Profile.Name;
            meta.EntityName = "Silicon test name";
        }
        else
            meta.EntityName = "silicon job name";

        _identity.QueueIdentityUpdate(mob);
    }
}
