using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Content.Shared.SimpleStation14.Species.Shadekin.Events;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;

namespace Content.Client.SimpleStation14.Species.Shadekin.Systems
{
    public sealed class ShadekinDarkSwappedSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private IgnoreHumanoidWithComponentOverlay _ignoreOverlay = default!;
        private EtherealOverlay _etherealOverlay = default!;

        public override void Initialize()
        {
            base.Initialize();

            _ignoreOverlay = new();
            _ignoreOverlay.ignoredComponents.Add(new HumanoidAppearanceComponent());
            _ignoreOverlay.allowAnywayComponents.Add(new ShadekinDarkSwappedComponent());
            _etherealOverlay = new();

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
            if (_player.LocalPlayer?.ControlledEntity != uid) return;

            _overlayMan.AddOverlay(_ignoreOverlay);
            _overlayMan.AddOverlay(_etherealOverlay);
        }

        private void OnShutdown(EntityUid uid, ShadekinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (_player.LocalPlayer?.ControlledEntity != uid) return;

            _ignoreOverlay.Reset();
            _overlayMan.RemoveOverlay(_ignoreOverlay);
            _overlayMan.RemoveOverlay(_etherealOverlay);
        }

        private void OnPlayerAttached(EntityUid uid, ShadekinDarkSwappedComponent component, PlayerAttachedEvent args)
        {
            _overlayMan.AddOverlay(_ignoreOverlay);
            _overlayMan.AddOverlay(_etherealOverlay);
        }

        private void OnPlayerDetached(EntityUid uid, ShadekinDarkSwappedComponent component, PlayerDetachedEvent args)
        {
            _ignoreOverlay.Reset();
            _overlayMan.RemoveOverlay(_ignoreOverlay);
            _overlayMan.RemoveOverlay(_etherealOverlay);
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
            }
            else
            {
                RemComp<ShadekinDarkSwappedComponent>(uid);
            }
        }
    }
}
