namespace Content.Server.SimpleStation14.Power.Components;

[RegisterComponent]
public class RandomBatteryChargeComponent : Component
{
    /// <summary>
    ///     The minimum and maximum max charge the battery can have.
    /// </summary>
    [DataField("batteryMaxMinMax")]
    public Vector2 BatteryMaxMinMax = (0.85f, 1.15f);

    /// <summary>
    ///     The minimum and maximum current charge the battery can have.
    /// </summary>
    [DataField("batteryChargeMinMax")]
    public Vector2 BatteryChargeMinMax = (1f, 1f);

    /// <summary>
    ///     True if the current charge is based on the preexisting current charge, or false if it's based on the max charge.
    /// </summary>
    [DataField("basedOnMaxCharge")]
    public bool BasedOnMaxCharge = true;
}
