using Content.Shared.Whitelist;

namespace Content.Shared.SimpleStation14.Interaction;

[RegisterComponent]
public sealed class CantInteractComponent : Component
{
    [DataField("unless")]
    public EntityWhitelist? Unless { get; set; } = default!;
}
