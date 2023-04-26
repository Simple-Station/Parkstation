namespace Content.Server.SimpleStation14.Silicon.Death;

[RegisterComponent]
public sealed class SiliconDownOnDeadComponent : Component
{
    public bool Dead { get; set; } = false;
}
