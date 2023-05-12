using System.Linq;
using Content.Server.Light.Components;
using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinDarkenSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowkinComponent, ComponentShutdown>(OnShutdown);
    }

    private static void OnStartup(EntityUid uid, ShadowkinComponent component, ComponentStartup args)
    {
        component.Darken = true;
    }

    private void OnShutdown(EntityUid uid, ShadowkinComponent component, ComponentShutdown args)
    {
        component.Darken = false;

        foreach (var light in component.DarkenedLights.ToArray())
        {
            var pointLight = _entity.GetComponent<PointLightComponent>(light);
            var shadowkinLight = _entity.GetComponent<ShadowkinLightComponent>(light);

            ResetLight(pointLight, shadowkinLight);
        }

        component.DarkenedLights.Clear();

        // I hate duplicate subscriptions
        _power.UpdateAlert(component.Owner, false);
    }


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

        var shadowkins = _entity.EntityQuery<ShadowkinComponent>();

        foreach (var shadowkin in shadowkins.Where(x => x.Darken))
        {
            if (!_entity.TryGetComponent(shadowkin.Owner, out ShadowkinDarkSwappedComponent? __) ||
                !_entity.TryGetComponent<TransformComponent>(shadowkin.Owner, out var transform))
                continue;

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

            var playerPos = _entity.GetComponent<TransformComponent>(shadowkin.Owner).WorldPosition;

            foreach (var light in shadowkin.DarkenedLights.ToArray())
            {
                var lightPos = _entity.GetComponent<TransformComponent>(light).WorldPosition;
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
