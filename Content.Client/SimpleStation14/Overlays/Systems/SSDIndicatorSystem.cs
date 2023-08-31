using Content.Shared.SimpleStation14.CCVar;
using Content.Shared.SimpleStation14.Overlays.SSDIndicator;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Client.SimpleStation14.Overlays;

public sealed class SSDIndicatorSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;


    public override void Update(float time)
    {
        if (!_config.GetCVar(SimpleStationCCVars.SSDIndicatorEnabled))
            return;

        var query = _entity.EntityQueryEnumerator<SSDIndicatorComponent>();

        while (query.MoveNext(out var uid, out var indicator))
        {
            // Update less often
            if (indicator.Updated)
            {
                indicator.Updated = false;
                continue;
            }
            indicator.Updated = true;


            var sprite = _entity.GetComponent<SpriteComponent>(uid);

            if (indicator.IsSSD)
            {
                // If the layer already exists, just make it visible
                if (sprite.LayerExists(SSDIndicatorLayer.SSDIndicator))
                {
                    sprite.LayerSetVisible(SSDIndicatorLayer.SSDIndicator, true);
                }
                // If the layer doesn't exist, create it, ensure it's visible
                else
                {
                    var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(indicator.RsiPath, indicator.RsiState));
                    sprite.LayerMapSet(SSDIndicatorLayer.SSDIndicator, layer);
                    sprite.LayerSetVisible(SSDIndicatorLayer.SSDIndicator, true);
                }
            }
            else
            {
                // If the layer exists, hide it, doesn't matter if it doesn't exist
                if (sprite.LayerExists(SSDIndicatorLayer.SSDIndicator))
                    sprite.LayerSetVisible(SSDIndicatorLayer.SSDIndicator, false);
            }
        }
    }
}

public enum SSDIndicatorLayer : byte
{
    SSDIndicator,
}
