using Content.Shared.SimpleStation14.SSDIndicator;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.SimpleStation14.Overlays;

public sealed class SSDIndicatorSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Update(float time)
    {
        var query = _entityManager.EntityQueryEnumerator<SSDIndicatorComponent>();

        while (query.MoveNext(out var uid, out var indicator))
        {
            var sprite = _entityManager.GetComponent<SpriteComponent>(uid);

            if (indicator.IsSSD)
            {
                if (sprite.LayerExists(SSDIndicatorLayer.SSDIndicator))
                {
                    sprite.LayerSetVisible(SSDIndicatorLayer.SSDIndicator, true);
                }
                else
                {
                    var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(indicator.RsiPath, indicator.RsiState));
                    sprite.LayerMapSet(SSDIndicatorLayer.SSDIndicator, layer);
                    sprite.LayerSetVisible(SSDIndicatorLayer.SSDIndicator, true);
                }
            }
            else
            {
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
