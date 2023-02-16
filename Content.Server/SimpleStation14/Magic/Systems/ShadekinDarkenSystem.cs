using Content.Server.Light.Components;
using Content.Shared.SimpleStation14.Magic.Components;
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

            SubscribeLocalEvent<ShadekinComponent, ComponentRemove>(OnRemove);
        }

        // todo: Set light color slightly darker than normal?
        // todo: Set light power dynamically too
        /// <summary>
        ///     This sucks, though isn't nearly as bad as the client to server networked version.
        /// </summary>
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var shadekins = _entityManager.EntityQuery<ShadekinComponent>();
            var _darkened = new List<EntityUid>();

            foreach (var shadekin in shadekins)
            {
                if (shadekin.Accumulator < shadekin.AccumulatorTime)
                {
                    shadekin.Accumulator += frameTime;
                    continue;
                }
                else shadekin.Accumulator = 0f;

                var entity = shadekin.Owner;
                var playerPos = _entityManager.GetComponent<TransformComponent>(entity).WorldPosition;

                var lightQuery = _entityManager.EntityQuery<PointLightComponent, ShadekinLightComponent>();
                foreach (var (light, __) in lightQuery)
                {
                    if (_darkened.Contains(light.Owner)) continue;
                    _darkened.Add(light.Owner);
                }

                shadekin.DarkenedLights = _darkened;

                foreach (var light in _darkened.ToArray())
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
                    if (distance > shadekin.DarkenRange || !shadekin.Darken)
                    {
                        ResetLight(pointLight, shadekinLight);
                        continue;
                    }


                    if (!shadekinLight.OldRadiusEdited)
                    {
                        shadekinLight.OldRadius = pointLight.Radius;
                        shadekinLight.OldRadiusEdited = true;
                    }

                    var radius = distance * 2f;
                    if (radius > shadekinLight.OldRadius) radius = shadekinLight.OldRadius;
                    if (radius < shadekinLight.OldRadius * 0.2f) radius = shadekinLight.OldRadius * 0.2f;

                    lightSys.SetRadius(pointLight.Owner, radius);
                }

                foreach (var light in _darkened.ToArray()) _darkened.Remove(light);
            }
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
