using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Damage;
using Content.Shared.Item;
using Content.Shared.Tag;

namespace Content.Shared.SimpleStation14.Hologram
{
    public class SharedHologramSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tagSystem = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<HologramComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        private void OnInteractionAttempt(EntityUid uid, HologramComponent component, InteractionAttemptEvent args)
        {
            if (args.Target == null)
                return;

            if (TryComp<DamageableComponent>(args.Target, out var dmg) && dmg.DamageContainerID == "Biological")
                args.Cancel();

            if (HasComp<ItemComponent>(args.Target) && !HasComp<UnremoveableComponent>(args.Target)
                && !_tagSystem.HasAnyTag(args.Target.Value, "Hardlight"))
                args.Cancel();
        }
    }
}
