using Content.Shared.SimpleStation14.Magic.Asclepius.Components;
using Content.Shared.SimpleStation14.Magic.Asclepius.Events;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.Magic.Asclepius.Systems
{
    public sealed class AsclepiusBindingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SpriteSystem _spriteSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<HippocraticOathCompleteEvent>(OnComplete);
            SubscribeLocalEvent<AsclepiusStaffComponent, ComponentStartup>(OnStartup);
        }

        private void OnComplete(HippocraticOathCompleteEvent args)
        {
            if (args.Cancelled) return;

            // How did you do the oath?
            if (!_entityManager.TryGetComponent<AsclepiusStaffComponent>(args.Staff, out var component))
            {
                return;
            }

            // Update the sprite
            if (_entityManager.TryGetComponent<SpriteComponent>(args.Staff, out var sprite))
            {
                if (component.BoundTo == EntityUid.Invalid)
                {
                    SetSprite(args.Staff, sprite, false);
                }
                else
                {
                    SetSprite(args.Staff, sprite, true);
                }
            }
        }

        private void OnStartup(EntityUid uid, AsclepiusStaffComponent component, ComponentStartup args)
        {
            if (_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite))
            {
                if (component.BoundTo == EntityUid.Invalid)
                {
                    SetSprite(uid, sprite, false);
                }
                else
                {
                    SetSprite(uid, sprite, true);
                }
            }
        }

        private void SetSprite(EntityUid uid, SpriteComponent component, bool active = true)
        {
            component.LayerSetState(0, active ? "active" : "dormant");
        }
    }
}
