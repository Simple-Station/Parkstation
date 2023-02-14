using Content.Shared.Tag;
using Content.Shared.SimpleStation14.Hologram;
using Robust.Client.Player;
using Content.Shared.Interaction.Helpers;


namespace Content.Client.SimpleStation14.Hologram;

public class HologramSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        // SubscribeLocalEvent<HologramComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalPlayer?.ControlledEntity == null) return;
        var uid = _player.LocalPlayer.ControlledEntity.Value;

        if (!_entityManager.TryGetComponent(uid, out HologramComponent? component)) return;

        var playerPos = _entityManager.GetComponent<TransformComponent>(uid).WorldPosition;
        var projQuery = _entityManager.EntityQuery<HoloProjectorComponent>();

        var projUid = EntityUid.Invalid;
        foreach (var proj in projQuery)
        {
            if (!proj.Owner.InRangeUnOccluded(uid)) continue;
            projUid = proj.Owner;
            break;
        }

        component.CurProjector = projUid;
    }
}
