using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.SimpleStation14.Magic.Events;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;

namespace Content.Client.SimpleStation14.Magic.Systems;
public sealed class ShadekinDarken : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private ShadekinDarkenOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();

        SubscribeNetworkEvent<ShadekinDarkSwappedEvent>(DarkSwap);

        SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadekinDarkSwappedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadekinDarkSwappedComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShadekinDarkSwappedComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void DarkSwap(ShadekinDarkSwappedEvent args)
    {
        ToggleInvisibility(args.Performer, args.IsDark);
    }


    private void OnStartup(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentStartup args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid) _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlay.Reset();
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnPlayerAttached(EntityUid uid, ShadekinDarkSwappedComponent component, PlayerAttachedEvent args)
    {
        ToggleInvisibility(uid, true); // TODO: Why doesn't this event emit?
    }

    private void OnPlayerDetached(EntityUid uid, ShadekinDarkSwappedComponent component, PlayerDetachedEvent args)
    {
        ToggleInvisibility(uid, false);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        ToggleInvisibility(_player.LocalPlayer?.ControlledEntity ?? EntityUid.Invalid, false);
    }


    public void ToggleInvisibility(EntityUid uid, bool isDark)
    {
        if (isDark)
        {
            EnsureComp<ShadekinDarkSwappedComponent>(uid);
            _overlayMan.AddOverlay(_overlay);
        }
        else
        {
            RemComp<ShadekinDarkSwappedComponent>(uid);
            _overlay.Reset();
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
