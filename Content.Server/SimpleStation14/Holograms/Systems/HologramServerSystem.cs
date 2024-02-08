using Content.Server.Humanoid;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Power.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Interaction;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Content.Shared.Movement.Systems;
using Content.Server.Mind.Components;
using Content.Shared.SimpleStation14.Holograms.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Server.EUI;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Holograms;

public sealed class HologramServerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly HologramSystem _hologram = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public const string TagHoloDisk = "HoloDisk";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HologramServerComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<HologramServerComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<HologramDiskComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HologramServerComponent, PowerChangedEvent>(OnPowerChanged);
    }

    /// <summary>
    ///     Handles generating a hologram from an inserted disk
    /// </summary>
    private void OnEntInserted(EntityUid uid, HologramServerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.DiskSlot || !_tags.HasTag(args.Entity, TagHoloDisk))
            return;

        if (Exists(component.LinkedHologram))
            if (!_hologram.TryKillHologram(component.LinkedHologram.Value))
                return; // This is a weird situation to encounter, so we'll just stop doin stuff.

        if (TryGenerateHologram(uid, args.Entity, out var holo, component))
        {
            component.LinkedHologram = holo;
            EnsureComp<HologramServerLinkedComponent>(holo.Value).LinkedServer = uid;
        }
    }

    /// <summary>
    ///     Handles killing a hologram when a disk is removed
    /// </summary>
    private void OnEntRemoved(EntityUid uid, HologramServerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.DiskSlot || !_tags.HasTag(args.Entity, "HoloDisk"))
            return;

        if (Exists(component.LinkedHologram))
            _hologram.DoKillHologram(component.LinkedHologram.Value);
    }

    /// <summary>
    ///     Called when the server's power state changes
    /// </summary>
    /// <param name="uid">The entity uid of the server</param>
    /// <param name="component">The HologramServerComponent</param>
    /// <param name="args">The PowerChangedEvent</param>
    private void OnPowerChanged(EntityUid uid, HologramServerComponent component, ref PowerChangedEvent args)
    {
        // If the server is no longer powered and the hologram exists
        if (!args.Powered && Exists(component.LinkedHologram))
        {
            // Kill the Hologram
            _hologram.DoKillHologram(component.LinkedHologram.Value);
            component.LinkedHologram = null;
        }

        // If the server is powered
        else if (args.Powered)
        {
            if (component.DiskSlot == null)
                return; // No disk slot

            var container = Comp<ContainerManagerComponent>(uid).Containers[component.DiskSlot];

            if (container.ContainedEntities.Count <= 0)
                return; // No disk in the server

            // If the hologram is generated successfully
            if (TryGenerateHologram(uid, container.ContainedEntities[0], out var holo, component))
            {
                // Set the linked hologram to the generated hologram
                var holoLinkComp = EnsureComp<HologramServerLinkedComponent>(holo.Value);
                component.LinkedHologram = holo;
                holoLinkComp.LinkedServer = uid;
            }
        }
    }

    public bool TryGenerateHologram(EntityUid server, EntityUid disk, [NotNullWhen(true)] out EntityUid? hologram, HologramServerComponent? holoServerComp = null)
    {
        hologram = null;

        // if (TryComp<HologramDiskDummyComponent>(disk, out var diskDummyComp)) //TODO

        if (!TryComp<HologramDiskComponent>(disk, out var diskComp) || diskComp.HoloMind == null)
            return false;

        return _hologram.TryGenerateHologram(diskComp.HoloMind, _transform.GetMoverCoordinates(server), out hologram);
    }

    private void OnAfterInteract(EntityUid uid, HologramDiskComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !TryComp<MindContainerComponent>(args.Target, out var targetMind))
            return;

        if (targetMind.Mind == null)
        {
            _popup.PopupEntity(Loc.GetString("system-hologram-disk-mind-none"), args.Target.Value, args.User);
            args.Handled = true;

            return;
        }

        component.HoloMind = targetMind.Mind;
        _popup.PopupEntity(Loc.GetString("system-hologram-disk-mind-saved"), args.Target.Value, args.User);

        args.Handled = true;
    }
}
