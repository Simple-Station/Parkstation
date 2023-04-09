using Content.Server.Jobs;
using Content.Server.SimpleStation14.Traits.Events;

namespace Content.Server.SimpleStation14.Jobs;

/// <summary>
///     This handles the adding of components to a player when they are cloned.
/// </summary>
public sealed class AddComponentSpecialSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeenClonedEvent>(OnBeenCloned);
    }

    /// <summary>
    ///     When the player is cloned, add all trait components selected during character creation
    /// </summary>
    private void OnBeenCloned(BeenClonedEvent args)
    {
        if (args.Mind.CurrentJob == null)
            return;

        foreach (var special in args.Mind.CurrentJob.Prototype.Special)
        {
            if (special is AddComponentSpecial)
                special.AfterEquip(args.Mob);
        }
    }
}
