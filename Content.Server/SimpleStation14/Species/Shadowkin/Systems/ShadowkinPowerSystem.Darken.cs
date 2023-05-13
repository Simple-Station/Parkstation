using System.Linq;
using Content.Server.Light.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinDarkenSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;


    public void ResetLight(PointLightComponent light, ShadowkinLightComponent sLight)
    {
        if (sLight.OldRadiusEdited)
            _light.SetRadius(light.Owner, sLight.OldRadius);
        sLight.OldRadiusEdited = false;

        if (sLight.OldEnergyEdited)
            light.Energy = sLight.OldEnergy;
        sLight.OldEnergyEdited = false;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shadowkins = _entity.EntityQueryEnumerator<ShadowkinDarkSwappedComponent>();

        while (shadowkins.MoveNext(out var uid, out var shadowkin))
        {
            if (!shadowkin.Darken)
                continue;

            var transform = Transform(uid);

            shadowkin.DarkenAccumulator -= frameTime;
            if (shadowkin.DarkenAccumulator > 0f)
                continue;
            shadowkin.DarkenAccumulator += shadowkin.DarkenRate;


            var darkened = new List<EntityUid>();
            var lightQuery = _lookup.GetEntitiesInRange(transform.MapID, transform.WorldPosition, shadowkin.DarkenRange, flags: LookupFlags.StaticSundries)
                .Where(x => _entity.HasComponent<ShadowkinLightComponent>(x) && _entity.HasComponent<PointLightComponent>(x));

            foreach (var entity in lightQuery)
            {
                if (!darkened.Contains(entity))
                    darkened.Add(entity);
            }

            _random.Shuffle(darkened);
            shadowkin.DarkenedLights = darkened;

            var playerPos = Transform(uid).WorldPosition;

            foreach (var light in shadowkin.DarkenedLights.ToArray())
            {
                var lightPos = Transform(light).WorldPosition;
                var pointLight = _entity.GetComponent<PointLightComponent>(light);


                if (!_entity.TryGetComponent(light, out ShadowkinLightComponent? shadowkinLight))
                    continue;
                if (!_entity.TryGetComponent(light, out PoweredLightComponent? powered) || !powered.On)
                {
                    ResetLight(pointLight, shadowkinLight);
                    continue;
                }


                if (!shadowkinLight.OldRadiusEdited)
                {
                    shadowkinLight.OldRadius = pointLight.Radius;
                    shadowkinLight.OldRadiusEdited = true;
                }
                if (!shadowkinLight.OldEnergyEdited)
                {
                    shadowkinLight.OldEnergy = pointLight.Energy;
                    shadowkinLight.OldEnergyEdited = true;
                }

                var distance = (lightPos - playerPos).Length;

                var radius = distance * 2f;
                if (shadowkinLight.OldRadiusEdited && radius > shadowkinLight.OldRadius)
                    radius = shadowkinLight.OldRadius;
                if (shadowkinLight.OldRadiusEdited && radius < shadowkinLight.OldRadius * 0.20f)
                    radius = shadowkinLight.OldRadius * 0.20f;

                var energy = distance * 0.8f;
                if (shadowkinLight.OldEnergyEdited && energy > shadowkinLight.OldEnergy)
                    energy = shadowkinLight.OldEnergy;
                if (shadowkinLight.OldEnergyEdited && energy < shadowkinLight.OldEnergy * 0.20f)
                    energy = shadowkinLight.OldEnergy * 0.20f;

                _light.SetRadius(pointLight.Owner, radius);
                pointLight.Energy = energy;
            }
        }
    }
}
