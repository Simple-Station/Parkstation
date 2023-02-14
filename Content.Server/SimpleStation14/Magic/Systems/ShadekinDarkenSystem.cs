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
                if (!_entityManager.TryGetComponent(light, out PoweredLightComponent? powered)) continue;
                if (!_entityManager.TryGetComponent(light, out ShadekinLightComponent? shadekinLight)) continue;
                if (!powered.On) continue;


                var lightPos = _entityManager.GetComponent<TransformComponent>(light).WorldPosition;
                var pointLight = _entityManager.GetComponent<PointLightComponent>(light);
                var lightSys = _systemManager.GetEntitySystem<SharedPointLightSystem>();

                var distance = (lightPos - playerPos).Length;
                if (distance > component.DarkenRange)
                {
                    lightSys.SetRadius(pointLight.Owner, shadekinLight.OldRadius);
                    shadekinLight.OldRadiusEdited = false;
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
    }
}
