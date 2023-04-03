namespace Content.Server.SimpleStation14.Speech.Components;

[RegisterComponent]
public sealed class VoiceOfGodComponent : Component
{
    [DataField("sound"), ViewVariables(VVAccess.ReadWrite)]
    public string Sound { get; set; } = "/Audio/SimpleStation14/Voice/voice_of_god.ogg";

    [DataField("volume"), ViewVariables(VVAccess.ReadWrite)]
    public int Volume { get; set; } = 3;


    [DataField("accent"), ViewVariables(VVAccess.ReadWrite)]
    public bool Accent { get; set; } = true;


    [DataField("color"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatColor { get; set; } = Color.Red.ToHex();

    [DataField("locale"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatLoc { get; set; } = "chat-manager-entity-say-god-wrap-message";
}
