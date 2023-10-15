using Content.Shared.SimpleStation14.Holograms;
using Content.Shared.SimpleStation14.Holograms.Components;
using Robust.Client.Player;

namespace Content.Client.SimpleStation14.Holograms;

public sealed class HologramSystem : SharedHologramSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Update(float frameTime)
    {
        var player = _player.LocalPlayer?.ControlledEntity;
        if (TryComp<HologramProjectedComponent>(player, out var holoProjComp))
            ProjectedUpdate(player.Value, holoProjComp, frameTime);
    }
}
