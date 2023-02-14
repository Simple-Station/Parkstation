using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.SimpleStation14.Magic.Events;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;

namespace Content.Client.SimpleStation14.Magic.Systems
{
    /// <summary>
    ///     Holy shit this is so laggy, move to server entirely if possible to avoid network spam.
    /// </summary>
    [Obsolete("Move entirely to server if possible")]
    public sealed class ShadekinDarkenSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        List<EntityUid> _darkened = new();

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (_player.LocalPlayer?.ControlledEntity == null) return;
            var uid = _player.LocalPlayer.ControlledEntity.Value;

            if (!_entityManager.TryGetComponent(uid, out ShadekinComponent? component)) return;

            if (component.Accumulator < component.AccumulatorTime)
            {
                component.Accumulator += frameTime;
                return;
            }
            else component.Accumulator = 0f;

            var lightQuery = _entityManager.EntityQuery<PointLightComponent, ShadekinLightComponent>();

            foreach (var (light, __) in lightQuery)
            {
                var entity = light.Owner;

                if (_darkened.Contains(entity)) continue;

                _darkened.Add(entity);
            }

            RaiseNetworkEvent(new ShadekinDarkenEvent(uid, _darkened));

            foreach (var entity in _darkened.ToArray()) _darkened.Remove(entity);
        }
    }
}
