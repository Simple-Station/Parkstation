namespace Content.Server.SimpleStation14.Species.Shadowkin.Components;

[RegisterComponent]
public sealed class ShadowkinDarkSwapPowerComponent : Component
{
    /// <summary>
    ///     If the entity should be sent to the dark
    /// </summary>
    [DataField("invisible")]
    public bool Invisible = true;

    /// <summary>
    ///     If it should be pacified
    /// </summary>
    [DataField("pacify")]
    public bool Pacify = true;

    /// <summary>
    ///     If the entity should dim nearby lights when swapped
    /// </summary>
    [DataField("darken"), ViewVariables(VVAccess.ReadWrite)]
    public bool Darken = true;

    /// <summary>
    ///     How far to dim nearby lights
    /// </summary>
    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRange = 5f;

    /// <summary>
    ///     How fast to refresh nearby light dimming in seconds
    ///     Without this performance would be significantly worse
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float DarkenRate = 0.084f; // 1/12th of a second
}
