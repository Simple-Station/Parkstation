using Content.Shared.SimpleStation14.Traits;
using Robust.Server.GameObjects;
using Robust.Shared.Network;

namespace Content.Server.SimpleStation14.Traits;

public sealed class HeightAdjustedSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    Vector2 originalScale = Vector2.One;

    public override void Initialize()
    {
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentStartup>(SetupHeight);
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentRemove>(ResetHeight);
    }

    private void SetupHeight(EntityUid uid, HeightAdjustedComponent component, ComponentStartup args)
    {
        var sprite = _entityManager.GetComponent<SpriteComponent>(uid);

        EnsureComp<ScaleVisualsComponent>(uid);
        if (!_appearance.TryGetData(uid, ScaleVisuals.Scale, out var oldScale)) oldScale = Vector2.One;
        originalScale = (Vector2) oldScale;

        oldScale = (Vector2) oldScale * sprite.Scale;
        oldScale = (Vector2) oldScale * new Vector2(component.Height, component.Height);

        _appearance.SetData(uid, ScaleVisuals.Scale, oldScale);
        if (TryComp<SharedEyeComponent>(uid, out var eye)) eye.Zoom = (Vector2) oldScale;
    }

    private void ResetHeight(EntityUid uid, HeightAdjustedComponent component, ComponentRemove args)
    {
        _appearance.SetData(uid, ScaleVisuals.Scale, originalScale);
        if (TryComp<SharedEyeComponent>(uid, out var eye)) eye.Zoom = (Vector2) originalScale;
    }
}
