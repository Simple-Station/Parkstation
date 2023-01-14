using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.SimpleStation14.Traits
{
    public sealed class LightWeightTraitSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<LightWeightTraitComponent, ComponentStartup>(OnInit);
            SubscribeLocalEvent<LightWeightTraitComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnInit(EntityUid uid, LightWeightTraitComponent component, ComponentStartup args)
        {
            var MoveSpeed = _entityManager.EnsureComponent<MovementSpeedModifierComponent>(uid);

            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, MoveSpeed.BaseWalkSpeed + component.Sprint, MoveSpeed.BaseSprintSpeed + component.Walk, MoveSpeed.Acceleration, MoveSpeed);
        }

        private void OnShutdown(EntityUid uid, LightWeightTraitComponent component, ComponentShutdown args)
        {
            var MoveSpeed = _entityManager.GetComponent<MovementSpeedModifierComponent>(uid);

            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, MoveSpeed.BaseWalkSpeed - component.Sprint, MoveSpeed.BaseSprintSpeed - component.Walk, MoveSpeed.Acceleration, MoveSpeed);
        }
    }
}
