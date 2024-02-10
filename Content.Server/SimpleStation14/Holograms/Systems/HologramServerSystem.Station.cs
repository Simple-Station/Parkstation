using Content.Server.GameTicking;
using Content.Server.SimpleStation14.Holograms.Components;

namespace Content.Server.SimpleStation14.Holograms;

public sealed partial class HologramServerSystem
{
    private void InitializeStation()
    {
        SubscribeLocalEvent<StationHologramDatabaseComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(EntityUid player, StationHologramDatabaseComponent component, PlayerSpawnCompleteEvent args)
    {
    }
}

