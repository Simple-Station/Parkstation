using System.Linq;
using Content.Server.GameTicking.Rules;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;

namespace Content.Server.SimpleStation14.GameTicking.OldStation;

public sealed class OldStationRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override string Prototype => "OldStation";
    private OldStationRuleConfiguration _config = new();

    private EntityUid? _station;
    private MapId? _planet;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Started()
    {
        SpawnMap();
    }

    public override void Ended()
    {

    }

    private bool SpawnMap()
    {
        if (_station != null)
            return true; // Map is already loaded.

        var path = _config.OldStationMap;

        if (path == null)
        {
            Logger.ErrorS("OldStation", "No station map specified for OldStation!");
            return false;
        }

        var mapId = _mapManager.CreateMap();
        var options = new MapLoadOptions()
        {
            LoadMap = true,
        };

        if (!_map.TryLoad(mapId, path.ToString(), out var grids, options) || !grids.Any())
        {
            Logger.ErrorS("OldStation", $"Error loading map {path} for OldStation!");
            return false;
        }

        // Assume the first grid is the station grid.
        _station = grids[0];
        _planet = mapId;

        return true;
    }
}
