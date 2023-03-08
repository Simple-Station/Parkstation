// new component named punpuncomponent
namespace Content.Server.SimpleStation14.Punpun;

[RegisterComponent]
public class PunpunComponent : Component
{
    // EntityUid to store the owner of the component
    public EntityUid owner = EntityUid.Invalid;
}
