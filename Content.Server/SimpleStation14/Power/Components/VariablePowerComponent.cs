namespace Content.Server.SimpleStation14.Power.Components;

/// <summary>
///     Defines that the given entity should take a variable amount of power, depending on if it's being used.
/// </summary>
[RegisterComponent]
public sealed class VariablePowerComponent : Component
{
    [DataField("powerActiveMulti")]
    public float PowerActiveMulti = 5.0f;

    [DataField("powerIdleMulti")]
    public float PowerIdleMulti = 0.1f;

    public float AddedValue = 0f;

    public bool Active = false;
}
