using Content.Shared.SimpleStation14.Traits.Components;
using Robust.Shared.Network;

namespace Content.Shared.SimpleStation14.Traits;

public sealed class HeightAdjustedSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentStartup>(SetupHeight);
    }

    private void SetupHeight(EntityUid uid, HeightAdjustedComponent component, ComponentStartup args)
    {
        if (_netManager.IsClient && !uid.IsClientSide()) return;

        EnsureComp<ScaleVisualsComponent>(uid);
        if (!_appearance.TryGetData(uid, ScaleVisuals.Scale, out var oldScale)) oldScale = Vector2.One;

        _appearance.SetData(uid, ScaleVisuals.Scale, (Vector2) oldScale * new Vector2(component.Height, component.Height));
        if (TryComp<SharedEyeComponent>(uid, out var eye)) eye.Zoom = (Vector2) oldScale * new Vector2(component.Height, component.Height);
    }
}
