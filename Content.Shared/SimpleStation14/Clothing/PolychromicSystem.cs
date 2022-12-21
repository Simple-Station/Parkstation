namespace Content.Shared.SimpleStation14.Clothing;

public sealed class SharedPolychromicSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PolychromicComponent, ComponentStartup>(Startup);
    }

    private void Startup(EntityUid uid, PolychromicComponent component, ComponentStartup args)
    {

    }
}
