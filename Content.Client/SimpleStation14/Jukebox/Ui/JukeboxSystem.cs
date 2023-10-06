using Content.Shared.Emag.Systems;

namespace Content.Shared.SimpleStation14.Jukebox;

public sealed class SharedJukeBoxSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JukeboxComponent, GotEmaggedEvent>(OnEmagged);
    }

    /// <summary>
    ///     Handles setting the Jukebox's state to emagged.
    /// </summary>
    private void OnEmagged(EntityUid jukeBox, JukeboxComponent jukeboxComp, ref GotEmaggedEvent args)
    {
        args.Handled = true;
    }
}
