namespace Content.Server.SimpleStation14.Grenades;

[RegisterComponent]
public sealed class SpawnEntitiesOnTriggerComponent : Component
{
    [DataField("prototype")]
    public string? Prototype = null;

    [DataField("count")]
    public int Count = 1;

    [DataField("minCount")]
    public int? MinCount = null;

    [DataField("velocity")]
    public float? Velocity = null;

    [DataField("despawnTime")]
    public float? DespawnTime = null;
}
