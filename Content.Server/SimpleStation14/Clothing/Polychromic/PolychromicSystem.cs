using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Content.Shared.SimpleStation14.Clothing;

namespace Content.Server.SimpleStation14.Clothing;

public sealed class PolychromicSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PolychromicComponent, GetVerbsEvent<InteractionVerb>>(OnVerb);
    }

    private void OnVerb(EntityUid uid, PolychromicComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract) return;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = Loc.GetString("polychromic-component-verb-text"),
            IconTexture = "/Textures/Interface/VerbIcons/settings.svg.192dpi.png",
            Act = () => TryOpenUi(uid, args.User, component)
        });
    }

    private void TryOpenUi(EntityUid uid, EntityUid user, PolychromicComponent? component = null)
    {
        if (!Resolve(uid, ref component)) return;
        if (!TryComp(user, out ActorComponent? actor)) return;
        _uiSystem.TryToggleUi(uid, PolychromicUiKey.Key, actor.PlayerSession);
    }
}
