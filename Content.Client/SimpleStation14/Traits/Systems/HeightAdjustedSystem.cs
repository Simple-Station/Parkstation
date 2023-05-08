using Content.Shared.SimpleStation14.Traits;
using Robust.Client.GameObjects;

namespace Content.Client.SimpleStation14.Traits.Systems;

public sealed class HeightAdjustedSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentShutdown>(Shutdown);
    }

    /// <summary>
    ///     Sets the scale and density of the entity.
    /// </summary>
    private void Startup(EntityUid uid, HeightAdjustedComponent component, ComponentStartup args)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite))
            return;
        EnsureComp<ScaleVisualsComponent>(uid);

        var oldScale = GetScale(uid, sprite);
        component.OriginalScale = oldScale;
        var newScale = oldScale * new Vector2(component.Width, component.Height);
        component.NewScale = newScale;

        SetScale(uid, newScale);
    }

    /// <summary>
    ///     Resets the scale and density of the entity.
    /// </summary>
    private void Shutdown(EntityUid uid, HeightAdjustedComponent component, ComponentShutdown args)
    {
        SetScale(uid, component.OriginalScale);
    }


    /// <summary>
    ///     Gets the scale of the entity.
    /// </summary>
    /// <param name="uid">The entity to get the scale of.</param>
    /// <param name="sprite">The sprite component of the entity.</param>
    public Vector2 GetScale(EntityUid uid, SpriteComponent sprite)
    {
        if (!_appearance.TryGetData(uid, ScaleVisuals.Scale, out var oldScale))
            oldScale = Vector2.One;

        return (Vector2) oldScale * sprite.Scale;
    }

    /// <summary>
    ///     Sets the scale of the entity and its eye zoom level.
    /// </summary>
    /// <param name="uid">The entity to set the scale of.</param>
    /// <param name="scale">The (x, y) scale to set things to.</param>
    public void SetScale(EntityUid uid, Vector2 scale)
    {
        _appearance.SetData(uid, ScaleVisuals.Scale, scale);

        if (TryComp<SharedEyeComponent>(uid, out var eye))
            eye.Zoom = scale;
    }
}
