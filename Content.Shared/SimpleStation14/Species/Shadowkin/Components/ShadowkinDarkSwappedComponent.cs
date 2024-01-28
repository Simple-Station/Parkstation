using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Components;

[RegisterComponent, NetworkedComponent]
public sealed class ShadowkinDarkSwappedComponent : Component
{
    /// <summary>
    ///     If the entity should be sent to the dark
    /// </summary>
    /// <remarks>
    ///     This is also defined in the power component, this is if you want to use only some effects of the swap without a toggle
    /// </remarks>
    [DataField("invisible"), ViewVariables(VVAccess.ReadWrite)]
    public bool Invisible = true;

    /// <summary>
    ///     If it should be pacified
    /// </summary>
    /// <inheritdoc cref="Invisible"/>
    [DataField("pacify")]
    public bool Pacify = true;

    /// <summary>
    ///     If the entity should dim nearby lights when swapped
    /// </summary>
    /// <inheritdoc cref="Invisible"/>
    [DataField("darken"), ViewVariables(VVAccess.ReadWrite)]
    public bool Darken = true;


    /// <summary>
    ///     How far to dim nearby lights
    /// </summary>
    /// <inheritdoc cref="Invisible"/>
    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRange = 5f;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> DarkenedLights = new();

    /// <summary>
    ///     How fast to refresh nearby light dimming in seconds
    ///     Without this performance would be significantly worse
    /// </summary>
    /// <inheritdoc cref="Invisible"/>
    [ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRate = 0.084f; // 1/12th of a second

    [ViewVariables(VVAccess.ReadWrite)]
    public float DarkenAccumulator = 0f;
}
