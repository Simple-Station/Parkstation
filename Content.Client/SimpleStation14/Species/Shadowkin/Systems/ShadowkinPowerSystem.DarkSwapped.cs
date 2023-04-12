using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Client.SimpleStation14.Overlays;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;
using Robust.Client.GameObjects;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using TerraFX.Interop.Windows;

namespace Content.Client.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinDarkSwappedSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private IgnoreHumanoidWithComponentOverlay _ignoreOverlay = default!;
        private EtherealOverlay _etherealOverlay = default!;

        public override void Initialize()
        {
            base.Initialize();

            _ignoreOverlay = new IgnoreHumanoidWithComponentOverlay();
            _ignoreOverlay.ignoredComponents.Add(new HumanoidAppearanceComponent());
            _ignoreOverlay.allowAnywayComponents.Add(new ShadowkinDarkSwappedComponent());
            _etherealOverlay = new EtherealOverlay();

            SubscribeNetworkEvent<ShadowkinDarkSwappedEvent>(DarkSwap);

            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, PlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ShadowkinDarkSwappedComponent, PlayerDetachedEvent>(OnPlayerDetached);

            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }

        private void DarkSwap(ShadowkinDarkSwappedEvent args)
        {
            ToggleInvisibility(args.Performer, args.DarkSwapped);
        }


        private void OnStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
        {
            if (_player.LocalPlayer?.ControlledEntity != uid)
                return;

            AddOverlay();
        }

        private void OnShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
        {
            if (_player.LocalPlayer?.ControlledEntity != uid)
                return;

            RemoveOverlay();
        }

        private void OnPlayerAttached(EntityUid uid, ShadowkinDarkSwappedComponent component, PlayerAttachedEvent args)
        {
            AddOverlay();
        }

        private void OnPlayerDetached(EntityUid uid, ShadowkinDarkSwappedComponent component, PlayerDetachedEvent args)
        {
            RemoveOverlay();
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            ToggleInvisibility(_player.LocalPlayer?.ControlledEntity ?? EntityUid.Invalid, false);
        }


        private void ToggleInvisibility(EntityUid uid, bool isDark)
        {
            if (isDark)
            {
                EnsureComp<ShadowkinDarkSwappedComponent>(uid);
            }
            else
            {
                RemComp<ShadowkinDarkSwappedComponent>(uid);
            }
        }

        private void AddOverlay()
        {
            _overlayMan.AddOverlay(_ignoreOverlay);
            _overlayMan.AddOverlay(_etherealOverlay);
        }

        private void RemoveOverlay()
        {
            _ignoreOverlay.Reset();
            _overlayMan.RemoveOverlay(_ignoreOverlay);
            _overlayMan.RemoveOverlay(_etherealOverlay);
        }
    }
}
