using System.Numerics;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.SimpleStation14.Holograms.Components;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Map;

namespace Content.Client.SimpleStation14.Holograms;

public sealed class HologramSystem : SharedHologramSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HologramProjectedComponent, ComponentShutdown>(OnHoloProjectedShutdown);
    }

    public override void Update(float frameTime)
    {
        var player = _player.LocalPlayer?.ControlledEntity; // This makes it so only the currently controlled entity is predicted, assuming they're a hologram.
        if (TryComp<HologramProjectedComponent>(player, out var holoProjComp))
            ProjectedUpdate(player.Value, holoProjComp, frameTime);

        HandleProjectedEffects(EntityQueryEnumerator<HologramProjectedComponent>());
    }

    private void HandleProjectedEffects(EntityQueryEnumerator<HologramProjectedComponent> query)
    {
        while (query.MoveNext(out var hologram, out var holoProjectedComp))
        {
            if (!holoProjectedComp.DoProjectionEffect)
            {
                DeleteEffect(holoProjectedComp);
                continue;
            }

            if (!holoProjectedComp.CurrentlyInProjector || holoProjectedComp.CurProjector == null || !Exists(holoProjectedComp.CurProjector.Value))
            {
                DeleteEffect(holoProjectedComp);
                continue;
            }

            var projector = holoProjectedComp.CurProjector.Value;

            var holoXformComp = Transform(hologram);
            var holoCoords = _transform.GetMoverCoordinates(hologram, holoXformComp);

            var projXformComp = Transform(projector);
            var projCoords = _transform.GetMoverCoordinates(projector, projXformComp);

            if (holoCoords.EntityId != projCoords.EntityId) // ¯\_(ツ)_/¯
            {
                DeleteEffect(holoProjectedComp);
                continue;
            }

            // Determine a middle point between the hologram and the projector.
            var effectPos = (holoCoords.Position + projCoords.Position) / 2;
            // Offset the position a quarter tile towards the projector.
            effectPos += (projCoords.Position - holoCoords.Position).Normalized() * 0.25f;
            // Determine a rotation that points from the projector to the hologram.
            var effectRot = (holoCoords.Position - projCoords.Position).ToAngle() + -MathHelper.PiOver2;

            var effectCoords = new EntityCoordinates(holoCoords.EntityId, effectPos);
            if (!effectCoords.IsValid(EntityManager))
            {
                DeleteEffect(holoProjectedComp);
                continue;
            }

            // Set or spawn the effect.
            if (holoProjectedComp.EffectEntity == null || !Exists(holoProjectedComp.EffectEntity.Value))
                holoProjectedComp.EffectEntity = Spawn(holoProjectedComp.EffectPrototype, effectCoords);
            else
                _transform.SetLocalPosition(holoProjectedComp.EffectEntity.Value, effectPos);

            _transform.SetLocalRotation(holoProjectedComp.EffectEntity.Value, effectRot);

            // Determine the scaling factor to make it fit between the hologram and the projector.
            var effectScale = new Vector2(1, (holoCoords.Position - projCoords.Position).Length());
            Comp<SpriteComponent>(holoProjectedComp.EffectEntity.Value).Scale = effectScale.Y > 0.1f ? effectScale : Vector2.One;
        }
    }

    private void OnHoloProjectedShutdown(EntityUid uid, HologramProjectedComponent component, ComponentShutdown args)
    {
        DeleteEffect(component);
    }

    private void DeleteEffect(HologramProjectedComponent component)
    {
        if (component.EffectEntity != null && Exists(component.EffectEntity.Value))
            QueueDel(component.EffectEntity.Value);

        component.EffectEntity = null;
    }
}
