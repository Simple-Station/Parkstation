using Content.Shared.SimpleStation14.Holograms.Components;

namespace Content.Shared.SimpleStation14.Holograms;

public partial class SharedHologramSystem
{
    private void InitializeServerLinked()
    {
        SubscribeLocalEvent<HologramServerLinkedComponent, ChangedGridEvent>(OnGridChange);
    }

    private void OnGridChange(EntityUid hologram, HologramServerLinkedComponent serverLinkComp, ref ChangedGridEvent args)
    {
        if (serverLinkComp.GridBound && serverLinkComp.LinkedServer != null && args.NewGrid != Transform(serverLinkComp.LinkedServer.Value).GridUid)
            DoReturnHologram(hologram);
    }
}
