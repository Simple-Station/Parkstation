using Content.Server.GameTicking.Rules.Configurations;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.GameTicking.OldStation;

public sealed class OldStationRuleConfiguration : GameRuleConfiguration
{
    public override string Id => "OldStation";

    [DataField("minPlayers")]
    public int MinPlayers = 8;

    [DataField("map", customTypeSerializer: typeof(ResourcePathSerializer))]
    public ResourcePath? OldStationMap = new("/Maps/SimpleStation14/oldstation.yml");
}
