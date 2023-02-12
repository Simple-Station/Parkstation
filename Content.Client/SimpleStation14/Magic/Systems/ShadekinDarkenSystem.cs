using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Magic.Components;
using Content.Shared.SimpleStation14.Magic.Events;

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
    }

    //////////////////////////////////////////

    private void DarkSwap(ShadekinDarkSwappedEvent args)
    {
        ToggleInvisibility(args.Performer, args.IsDark);
    }

    public void ToggleInvisibility(EntityUid uid, bool isDark)
    {
        if (isDark)
        {
            EnsureComp<ShadekinDarkSwappedComponent>(uid);
            _overlayMan.AddOverlay(_overlay);
        }
        else if (!isDark)
        {
            RemComp<ShadekinDarkSwappedComponent>(uid);
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
