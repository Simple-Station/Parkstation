namespace Content.Server.SimpleStation14.Holograms.CctvDatabase;

[RegisterComponent]
public sealed class StationCctvDatabaseComponent : Component
{
    public List<Mind.Mind> PotentialsList = new();
}
