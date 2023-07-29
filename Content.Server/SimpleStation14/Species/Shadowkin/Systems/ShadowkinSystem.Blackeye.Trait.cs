using Content.Server.SimpleStation14.Species.Shadowkin.Components;
using Content.Shared.SimpleStation14.Species.Shadowkin.Events;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Systems;

public sealed class ShadowkinBlackeyeTraitSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinBlackeyeTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ShadowkinBlackeyeTraitComponent _, ComponentStartup args)
    {
        RaiseLocalEvent(uid, new ShadowkinBlackeyeEvent(uid, false));
        RaiseNetworkEvent(new ShadowkinBlackeyeEvent(uid, false));
    }
}
