using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Robust.Shared.Random;
using Content.Server.Station.Systems;

namespace Content.Server.SiliconJobMetadata;

public sealed class PlayerSiliconJobMetadataSystem : EntitySystem
{
    [Dependency] private readonly IdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerMobSpawnedEvent>(OnSpawnPlayer);
    }

    private void OnSpawnPlayer(PlayerMobSpawnedEvent args)
    {
        Logger.Debug("SiliconJobMetadataSystem.OnSpawnPlayer");
        var mob = args.SpawnResult;
        var meta = MetaData(mob);

        if (args.Profile != null)
        {
            meta.EntityName = args.Profile.Name;
        }
        else
            meta.EntityName = "No Name Silicon"; // TODO: Use random name?

        _identity.QueueIdentityUpdate(mob);
    }
}
