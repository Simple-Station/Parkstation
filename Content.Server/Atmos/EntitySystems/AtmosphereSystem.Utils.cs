using System.Runtime.CompilerServices;
using Content.Server.Atmos.Components;
using Content.Server.Cargo.Systems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Maps; // Parkstation-ReinforcedReinforcedTiles
using Robust.Shared.Map.Components;
using Dependency = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server.Atmos.EntitySystems;

public partial class AtmosphereSystem
{
    [Dependency] private readonly PricingSystem _pricingSystem = default!;

    /// <summary>
    /// Gets the particular price of an air mixture.
    /// </summary>
    public double GetPrice(GasMixture mixture, bool sale = false)
    {
        double basePrice = 0; // moles of gas * price/mole
        float totalMoles = 0; // total number of moles in can
        float maxComponent = 0; // moles of the dominant gas
        for (var i = 0; i < Atmospherics.TotalNumberOfGases; i++)
        {
            var gas = GetGas(i);
            var supply = _pricingSystem.GetGasSupply(gas);
            var demand = _pricingSystem.GetGasDemand(gas);

            basePrice += _pricingSystem.GetSupplyDemandPrice(mixture.Moles[i] * gas.PricePerMole, gas.HalfPriceSurplus, supply, demand);
            totalMoles += mixture.Moles[i];
            maxComponent = Math.Max(maxComponent, mixture.Moles[i]);

            if (sale)
                _pricingSystem.AddGasSupply(gas, mixture.Moles[i]);
        }

        // Pay more for gas canisters that are more pure
        float purity = 1;
        if (totalMoles > 0) {
            purity = maxComponent / totalMoles;
        }

        return basePrice * purity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InvalidateVisuals(EntityUid gridUid, Vector2i tile, GasTileOverlayComponent? comp = null)
    {
        _gasTileOverlaySystem.Invalidate(gridUid, tile, comp);
    }

    public bool NeedsVacuumFixing(MapGridComponent mapGrid, Vector2i indices)
    {
        var value = false;

        var enumerator = GetObstructingComponentsEnumerator(mapGrid, indices);

        while (enumerator.MoveNext(out var airtight))
        {
            value |= airtight.FixVacuum;
        }

        return value;
    }

    /// <summary>
    ///     Gets the volume in liters for a number of tiles, on a specific grid.
    /// </summary>
    /// <param name="mapGrid">The grid in question.</param>
    /// <param name="tiles">The amount of tiles.</param>
    /// <returns>The volume in liters that the tiles occupy.</returns>
    private float GetVolumeForTiles(MapGridComponent mapGrid, int tiles = 1)
    {
        return Atmospherics.CellVolume * mapGrid.TileSize * tiles;
    }

    /// <summary>
    ///     Gets all obstructing <see cref="AirtightComponent"/> instances in a specific tile.
    /// </summary>
    /// <param name="mapGrid">The grid where to get the tile.</param>
    /// <param name="tile">The indices of the tile.</param>
    /// <returns>The enumerator for the airtight components.</returns>
    public AtmosObstructionEnumerator GetObstructingComponentsEnumerator(MapGridComponent mapGrid, Vector2i tile)
    {
        var ancEnumerator = mapGrid.GetAnchoredEntitiesEnumerator(tile);
        var airQuery = GetEntityQuery<AirtightComponent>();

        var enumerator = new AtmosObstructionEnumerator(ancEnumerator, airQuery);
        return enumerator;
    }

    private AtmosDirection GetBlockedDirections(MapGridComponent mapGrid, Vector2i indices)
    {
        var value = AtmosDirection.Invalid;

        var enumerator = GetObstructingComponentsEnumerator(mapGrid, indices);

        while (enumerator.MoveNext(out var airtight))
        {
            if(airtight.AirBlocked)
                value |= airtight.AirBlockedDirection;
        }

        return value;
    }

    /// <summary>
    ///     Pries a tile in a grid.
    /// </summary>
    /// <param name="mapGrid">The grid in question.</param>
    /// <param name="tile">The indices of the tile.</param>
    private void PryTile(MapGridComponent mapGrid, Vector2i tile)
    {
        if (!mapGrid.TryGetTileRef(tile, out var tileRef) || tileRef.Tile.GetContentTileDefinition().Reinforced) // Parkstation-ReinforcedReinforcedTiles
            return;

        _tile.PryTile(tileRef);
    }
}
