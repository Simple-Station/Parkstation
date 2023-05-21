using Content.Server.Ghost.Components;
using Content.Server.Visible;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Eye
{
    /// <summary>
    ///     Place to handle eye component startup for whatever systems.
    /// </summary>
    public sealed class EyeStartup : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EyeComponent, ComponentStartup>(OnEyeStartup);
        }

        private void OnEyeStartup(EntityUid uid, EyeComponent component, ComponentStartup args)
        {
            if (!_entityManager.HasComponent<GhostComponent>(uid)) return;

            component.VisibilityMask |= (uint) VisibilityFlags.AIEye;
        }
    }
}
