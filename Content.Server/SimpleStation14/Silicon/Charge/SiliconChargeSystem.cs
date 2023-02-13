using Robust.Shared.Random;
using Content.Shared.Silicon.Charge;

namespace Content.Server.Silicon.Charge;

public sealed class SiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var silicon in EntityQuery<SiliconChargeComponent>())
        {
            if (silicon.CurrentCharge > 0)
            {
                silicon.CurrentCharge -= (frameTime * silicon.ChargeDrainMult * 2) * _random.NextFloat(1.1f, 0.9f);
            }
        }
    }
}
