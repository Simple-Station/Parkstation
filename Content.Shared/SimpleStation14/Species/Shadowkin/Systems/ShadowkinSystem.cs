using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Species.Shadowkin.Systems
{
    public sealed class ShadowkinSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShadowkinComponent, ComponentGetState>(GetCompState);
            SubscribeLocalEvent<ShadowkinComponent, ComponentHandleState>(HandleCompState);
        }


        private void GetCompState(EntityUid uid, ShadowkinComponent component, ref ComponentGetState args)
        {
            args.State = new ShadowkinComponentState
            {
                PowerLevel = component.PowerLevel,
                PowerLevelGain = component.PowerLevelGain,
                PowerLevelGainMultiplier = component.PowerLevelGainMultiplier,
                PowerLevelGainEnabled = component.PowerLevelGainEnabled,
                Blackeye = component.Blackeye
            };
        }

        private void HandleCompState(EntityUid uid, ShadowkinComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not ShadowkinComponentState shadowkin)
            {
                return;
            }

            component.PowerLevel = shadowkin.PowerLevel;
            component.PowerLevelGain = shadowkin.PowerLevelGain;
            component.PowerLevelGainMultiplier = shadowkin.PowerLevelGainMultiplier;
            component.PowerLevelGainEnabled = shadowkin.PowerLevelGainEnabled;
            component.Blackeye = shadowkin.Blackeye;
        }
    }
}
