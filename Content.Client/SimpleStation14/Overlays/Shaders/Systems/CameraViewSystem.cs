///////// Why is this file here? The game can't build, why is this on master? You made a branch for this. - death

// using Content.Shared.CameraView;
// using Content.Shared.CameraView.Components;
// using Robust.Client.GameObjects;
// using Robust.Shared.GameStates;

// namespace Content.Client.CameraView
// {
//     public sealed class CameraViewSystem : SharedCameraViewSystem
//     {
//         public override void Initialize()
//         {
//             base.Initialize();
//             SubscribeLocalEvent<CameraViewComponent, ComponentStartup>(OnViewStartup);
//             SubscribeLocalEvent<CameraViewComponent, ComponentShutdown>(OnViewShutdown);
//             SubscribeLocalEvent<CameraViewComponent, ComponentHandleState>(OnViewHandleState);
//         }

//         private void OnViewStartup(EntityUid uid, CameraViewComponent component, ComponentStartup args)
//         {
//             // Center the player's eye on the vehicle
//             if (TryComp(uid, out EyeComponent? eyeComp))
//                 eyeComp.Target ??= component.Vehicle;
//         }

//         private void OnViewShutdown(EntityUid uid, CameraViewComponent component, ComponentShutdown args)
//         {
//             // reset the riders eye centering.
//             if (TryComp(uid, out EyeComponent? eyeComp) && eyeComp.Target == component.Vehicle)
//                 eyeComp.Target = null;
//         }

//         private void OnViewHandleState(EntityUid uid, CameraViewComponent component, ref ComponentHandleState args)
//         {
//             if (args.Current is not CameraViewComponentState state)
//                 return;

//             if (TryComp(uid, out EyeComponent? eyeComp) && eyeComp.Target == component.Vehicle)
                // eyeComp.Target = state.Entity;

//             component.Vehicle = state.Entity;
//         }
//     }
// }
