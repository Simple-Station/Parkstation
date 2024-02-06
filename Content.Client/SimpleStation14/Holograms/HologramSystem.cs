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
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HologramProjectedComponent, ComponentShutdown>(OnProjectedShutdown);
    }

    public override void Update(float frameTime)
    {
        var player = _player.LocalPlayer?.ControlledEntity;
        if (TryComp<HologramProjectedComponent>(player, out var holoProjComp))
        {
            ProjectedUpdate(player.Value, holoProjComp, frameTime); // This makes it so only the currently controlled entity is predicted, assuming they're a hologram.

            // Check if we should be setting the eye target of the hologram.
            if (holoProjComp.SetEyeTarget && TryComp<EyeComponent>(player.Value, out var eyeComp))
                eyeComp.Target = holoProjComp.CurProjector;
        }

        HandleProjectedEffects(EntityQueryEnumerator<HologramProjectedComponent>());
    }

    private void OnProjectedShutdown(EntityUid hologram, HologramProjectedComponent component, ComponentShutdown args)
    {
        DeleteEffect(component);

        if (component.SetEyeTarget && TryComp<EyeComponent>(hologram, out var eyeComp))
            eyeComp.Target = null; // This should be fine? I guess if you're a hologram riding a vehicle when this happens it'd be a bit weird.
    }

    private void HandleProjectedEffects(EntityQueryEnumerator<HologramProjectedComponent> query)
    {
        while (query.MoveNext(out var hologram, out var holoProjectedComp))
        {
            if (holoProjectedComp.EffectPrototype == null)
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

            var originPos = projCoords.Position;

            // Add the effect's offset, if applicable.
            if (TryComp<HologramProjectorComponent>(projector, out var projComp))
            {
                var direction = projXformComp.LocalRotation.GetCardinalDir();

                var offset = direction switch
                {
                    Direction.North => projComp.EffectOffsets[Direction.South],
                    Direction.South => projComp.EffectOffsets[Direction.North],
                    Direction.East => projComp.EffectOffsets[Direction.West],
                    Direction.West => projComp.EffectOffsets[Direction.East],
                    _ => Vector2.Zero
                };

                originPos += offset;
            }

            // Determine a middle point between the hologram and the projector.
            var effectPos = (holoCoords.Position + originPos) / 2;

            // Determine a rotation that points from the projector to the hologram.
            var effectRot = (holoCoords.Position - originPos).ToAngle() - MathHelper.PiOver2;

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
            var yScale = (holoCoords.Position - originPos).Length();
            var effectScale = new Vector2(1, Math.Max(0.1f, yScale)); // No smaller than 0.1.
            Comp<SpriteComponent>(holoProjectedComp.EffectEntity.Value).Scale = effectScale;
        }
    }

    private void DeleteEffect(HologramProjectedComponent component)
    {
        if (component.EffectEntity != null && Exists(component.EffectEntity.Value))
            QueueDel(component.EffectEntity.Value);

        component.EffectEntity = null;
    }
}
