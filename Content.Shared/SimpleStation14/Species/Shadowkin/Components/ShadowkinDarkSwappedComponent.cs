using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Components;

[RegisterComponent, NetworkedComponent]
public sealed class ShadowkinDarkSwappedComponent : Component
{
    /// <summary>
    ///     For whatever random things want only the darken effect.
    /// </summary>
    [DataField("invisible")]
    public bool Invisible = true;

    [DataField("darken"), ViewVariables(VVAccess.ReadWrite)]
    public bool Darken = true;

    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRange = 5f;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> DarkenedLights = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRate = 0.084f; // 1/12th of a second

    [ViewVariables(VVAccess.ReadWrite)]
    public float DarkenAccumulator = 0f;
}
