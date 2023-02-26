// using Content.Client.Gravity;
// using Content.Shared.Anomaly;
// using Content.Shared.Anomaly.Components;
// using Robust.Client.GameObjects;
// using Robust.Shared.Timing;
// using Content.Shared.SimpleStation14.Hologram;

// namespace Content.Client.SimpleStation14.Hologram;

// public sealed class HologramSystem : SharedHologramSystem
// {
//     [Dependency] private readonly IGameTiming _timing = default!;
//     [Dependency] private readonly FloatingVisualizerSystem _floating = default!;

//     /// <inheritdoc/>
//     public override void Initialize()
//     {
//         base.Initialize();

//         SubscribeLocalEvent<HologramComponent, ComponentStartup>(OnStartup);
//         SubscribeLocalEvent<HologramComponent, AnimationCompletedEvent>(OnAnimationComplete);
//     }

//     private void OnStartup(EntityUid uid, HologramComponent component, ref ComponentStartup args)
//     {
//         _floating.FloatAnimation(uid, new Vector2(0f, 0.07f), "holofloat", 3);
//     }

//     private void OnAnimationComplete(EntityUid uid, HologramComponent component, AnimationCompletedEvent args)
//     {
//         if (args.Key != "holofloat")
//             return;
//         _floating.FloatAnimation(uid, new Vector2(0, 0.15f), "holofloat", 3.5f);
//     }
// }
