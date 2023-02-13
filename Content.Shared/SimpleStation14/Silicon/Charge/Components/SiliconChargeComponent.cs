namespace Content.Shared.Silicon.Charge;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed class SiliconChargeComponent : Component
{
    [DataField("currentCharge"), ViewVariables(VVAccess.ReadWrite)]
    public float CurrentCharge = 100;

    [DataField("maxCharge"), ViewVariables(VVAccess.ReadWrite)]
    public float MaxCharge = 2000;

    [DataField("chargeDrainMult"), ViewVariables(VVAccess.ReadWrite)]
    public float ChargeDrainMult = 1;

    [DataField("freezeOnEmpty")]
    public bool FreezeOnEmpty = false;
}
