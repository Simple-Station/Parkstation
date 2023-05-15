using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.Silicon.Charge;

[RegisterComponent]
public class BatteryDrinkerComponent : Component
{
    /// <summary>
    ///     Is this drinker allowed to drink batteries not tagged as <see cref="BatteryDrinkSource"/>?
    /// </summary>
    [DataField("drinkAll"), ViewVariables(VVAccess.ReadWrite)]
    public bool DrinkAll = false;

    /// <summary>
    ///     How long it takes to drink from the battery, in seconds.
    ///     Is mutliplied by the source.
    /// </summary>
    [DataField("drinkSpeed"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkSpeed = 1.5f;

    /// <summary>
    ///     The multiplier for the amount of power to attempt to drink.
    ///     Default amount is 1000
    /// </summary>
    [DataField("drinkMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkMultiplier = 5f;

    /// <summary>
    ///     The sound to override the standard drink sound with.
    ///     Uses per source sound if null.
    /// </summary>
    [DataField("drinkSoundOverride")]
    public SoundSpecifier? DrinkSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");

    /// <summary>
    ///     The localised string to display when drinking from a battery.
    ///     Doesn't _need_ to be localised, but come on, man.
    /// </summary>
    [DataField("drinkText")]
    public string DrinkText = "aaaaaaaaaa";
}
