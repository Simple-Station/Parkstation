using System.Linq;
using Content.Server.Light.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinDarkenSystem : EntitySystem
    {
        [Dependency] private readonly ShadekinPowerSystem _powerSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _systemManager = default!;
        [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadekinComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShadekinComponent, ComponentShutdown>(OnShutdown);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var shadekins = _entityManager.EntityQuery<ShadekinComponent>();

            foreach (var shadekin in shadekins.Where(x => x.Darken))
            {
                if (!_entityManager.TryGetComponent(shadekin.Owner, out ShadekinDarkSwappedComponent? __) ||
                    !_entityManager.TryGetComponent<TransformComponent>(shadekin.Owner, out var transform))
                    continue;

                shadekin.DarkenAccumulator += frameTime;
                if (shadekin.DarkenAccumulator < shadekin.DarkenRate) continue;
                shadekin.DarkenAccumulator = 0f;


                var _darkened = new List<EntityUid>();
                var lightQuery = _lookupSystem.GetEntitiesInRange(transform.MapID, transform.WorldPosition, shadekin.DarkenRange, flags: LookupFlags.StaticSundries)
                    .Where(x => _entityManager.HasComponent<ShadekinLightComponent>(x) && _entityManager.HasComponent<PointLightComponent>(x));

                foreach (var entity in lightQuery)
                {
                    if (!_darkened.Contains(entity)) _darkened.Add(entity);
                }

                _random.Shuffle(_darkened);
                shadekin.DarkenedLights = _darkened;

                var playerPos = _entityManager.GetComponent<TransformComponent>(shadekin.Owner).WorldPosition;

                foreach (var light in shadekin.DarkenedLights.ToArray())
                {
                    var lightPos = _entityManager.GetComponent<TransformComponent>(light).WorldPosition;
                    var pointLight = _entityManager.GetComponent<PointLightComponent>(light);


                    if (!_entityManager.TryGetComponent(light, out ShadekinLightComponent? shadekinLight)) continue;
                    if (!_entityManager.TryGetComponent(light, out PoweredLightComponent? powered) || !powered.On)
                    {
                        ResetLight(pointLight, shadekinLight);
                        continue;
                    }


                    if (!shadekinLight.OldRadiusEdited)
                    {
                        shadekinLight.OldRadius = pointLight.Radius;
                        shadekinLight.OldRadiusEdited = true;
                    }
                    if (!shadekinLight.OldEnergyEdited)
                    {
                        shadekinLight.OldEnergy = pointLight.Energy;
                        shadekinLight.OldEnergyEdited = true;
                    }

                    var distance = (lightPos - playerPos).Length;

                    var radius = distance * 2f;
                    if (shadekinLight.OldRadiusEdited && radius > shadekinLight.OldRadius) radius = shadekinLight.OldRadius;
                    if (shadekinLight.OldRadiusEdited && radius < shadekinLight.OldRadius * 0.20f) radius = shadekinLight.OldRadius * 0.20f;

                    var energy = distance * 0.8f;
                    if (shadekinLight.OldEnergyEdited && energy > shadekinLight.OldEnergy) energy = shadekinLight.OldEnergy;
                    if (shadekinLight.OldEnergyEdited && energy < shadekinLight.OldEnergy * 0.20f) energy = shadekinLight.OldEnergy * 0.20f;

                    _lightSystem.SetRadius(pointLight.Owner, radius);
                    pointLight.Energy = energy;
                }
            }
        }

        private void OnStartup(EntityUid uid, ShadekinComponent component, ComponentStartup args)
        {
            component.Darken = true;
        }

        private void OnShutdown(EntityUid uid, ShadekinComponent component, ComponentShutdown args)
        {
            component.Darken = false;

            foreach (var light in component.DarkenedLights.ToArray())
            {
                var pointLight = _entityManager.GetComponent<PointLightComponent>(light);
                var shadekinLight = _entityManager.GetComponent<ShadekinLightComponent>(light);

                ResetLight(pointLight, shadekinLight);
            }

            component.DarkenedLights.Clear();

            // I hate duplicate subscriptions
            _powerSystem.UpdateAlert(component.Owner, false);
        }


        public void ResetLight(PointLightComponent light, ShadekinLightComponent sLight)
        {
            if (sLight.OldRadiusEdited) _lightSystem.SetRadius(light.Owner, sLight.OldRadius);
            sLight.OldRadiusEdited = false;
            if (sLight.OldEnergyEdited) light.Energy = sLight.OldEnergy;
            sLight.OldEnergyEdited = false;
        }
    }
}
