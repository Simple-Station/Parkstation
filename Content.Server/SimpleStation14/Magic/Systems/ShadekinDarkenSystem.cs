using Content.Server.Light.Components;
using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.SimpleStation14.Magic.Events;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Magic.Systems
{
    public sealed class ShadekinDarkenSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _systemManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<ShadekinDarkenEvent>(OnDarken);

            SubscribeLocalEvent<ShadekinComponent, ComponentRemove>(OnRemove);
        }

        // todo: Set light color slightly darker than normal?
        // todo: Set light power dynamically too
        private void OnDarken(ShadekinDarkenEvent args)
        {
            var entity = args.Euid;
            var playerPos = _entityManager.GetComponent<TransformComponent>(entity).WorldPosition;
            var component = _entityManager.GetComponent<ShadekinComponent>(entity);

            foreach (var light in args.Lights)
            {
                var lightPos = _entityManager.GetComponent<TransformComponent>(light).WorldPosition;
                var pointLight = _entityManager.GetComponent<PointLightComponent>(light);
                var lightSys = _systemManager.GetEntitySystem<SharedPointLightSystem>();


                if (!_entityManager.TryGetComponent(light, out ShadekinLightComponent? shadekinLight)) continue;
                if (!_entityManager.TryGetComponent(light, out PoweredLightComponent? powered)
                    || !_entityManager.TryGetComponent(entity, out ShadekinDarkSwappedComponent? _)
                    || !powered.On)
                {
                    ResetLight(pointLight, shadekinLight);
                    continue;
                }


                var distance = (lightPos - playerPos).Length;
                if (distance > component.DarkenRange || !component.Darken)
                {
                    ResetLight(pointLight, shadekinLight);
                    continue;
                }


                if (!shadekinLight.OldRadiusEdited)
                {
                    shadekinLight.OldRadius = pointLight.Radius;
                    shadekinLight.OldRadiusEdited = true;
                }

                var radius = distance * 1.5f;
                if (radius > shadekinLight.OldRadius) radius = shadekinLight.OldRadius;
                if (radius < shadekinLight.OldRadius * 0.2f) radius = shadekinLight.OldRadius * 0.2f;

                lightSys.SetRadius(pointLight.Owner, radius);
            }

            component.DarkenedLights = args.Lights;
        }

        private void OnRemove(EntityUid uid, ShadekinComponent component, ComponentRemove args)
        {
            component.Darken = false;

            foreach (var light in component.DarkenedLights.ToArray())
            {
                var pointLight = _entityManager.GetComponent<PointLightComponent>(light);
                var shadekinLight = _entityManager.GetComponent<ShadekinLightComponent>(light);
                var lightSys = _systemManager.GetEntitySystem<SharedPointLightSystem>();

                lightSys.SetRadius(pointLight.Owner, shadekinLight.OldRadius);
                shadekinLight.OldRadiusEdited = false;
            }

            component.DarkenedLights.Clear();
        }


        private void ResetLight(PointLightComponent light, ShadekinLightComponent sLight)
        {
            var lightSys = _systemManager.GetEntitySystem<SharedPointLightSystem>();

            lightSys.SetRadius(light.Owner, sLight.OldRadius);
            sLight.OldRadiusEdited = false;
        }
    }
}
