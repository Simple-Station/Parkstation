using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Holograms;

[RegisterComponent, NetworkedComponent]
public sealed class HologramProjectorComponent : Component
{
    /// <summary>
    ///     The tile offset of the projector effect for this projector for each direction.
    /// </summary>
    [DataField("effectOffsets")]
    public Dictionary<Direction, Vector2> EffectOffsets { get; } = new() { { Direction.North, Vector2.Zero }, { Direction.East, Vector2.Zero }, { Direction.South, Vector2.Zero }, { Direction.West, Vector2.Zero } };
}
