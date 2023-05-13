using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinTintSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    private ColorTintOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new ColorTintOverlay
        {
            tintColor = new Vector3(0.5f, 0f, 0.5f),
            tintAmount = 0.25f,
            comp = new ShadowkinComponent()
        };

        SubscribeLocalEvent<ShadowkinComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowkinComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadowkinComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShadowkinComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnStartup(EntityUid uid, ShadowkinComponent component, ComponentStartup args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid) return;

        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, ShadowkinComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid) return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, ShadowkinComponent component, PlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ShadowkinComponent component, PlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var uid = _player.LocalPlayer?.ControlledEntity;
        if (uid == null)
            return;

        if (!_entity.TryGetComponent(uid, out ShadowkinComponent? comp))
            return;
        if (!_entity.TryGetComponent(uid, out SpriteComponent? sprite))
            return;
        if (!sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var index))
            return;
        if (!sprite.TryGetLayer(index, out var layer))
            return;

        // Eye color
        comp.TintColor = new Vector3(layer.Color.R, layer.Color.G, layer.Color.B);

        // 1/3 = 0.333...
        // intensity = min + (power / max)
        // intensity = intensity / 0.333
        // intensity = clamp intensity min, max
        const float min = 0.45f;
        const float max = 0.75f;
        comp.TintIntensity = Math.Clamp(min + (comp.PowerLevel / comp.PowerLevelMax) * 0.333f, min, max);

        UpdateShader(comp.TintColor, comp.TintIntensity);
    }


    private void UpdateShader(Vector3? color, float? intensity)
    {
        while (_overlayMan.HasOverlay<ColorTintOverlay>())
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        if (color != null)
            _overlay.tintColor = color;
        if (intensity != null)
            _overlay.tintAmount = intensity;

        _overlayMan.AddOverlay(_overlay);
    }
}
