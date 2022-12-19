using Content.Server.Mind.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Host)]
    public sealed class SlamOfTheNorthStar : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        // [Dependency] private readonly PolymorphableSystem _polymorphableSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public string Command => "slamOfTheNorthStar";
        public string Description => "Everyone is a basketball, everyone is bouncing like crazy, music is playing, have fun. (IRREVERSIBLE) (DOESNT WORK)";
        public string Help => "slamOfTheNorthStar";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // var player = shell.Player as IPlayerSession;
            // if (player == null) return;

        var _polymorphableSystem = _entities.EntitySysManager.GetEntitySystem<PolymorphableSystem>();

            foreach (var comp in _entities.EntityQuery<MindComponent>())
            {
                SoundSystem.Play("/Audio/SimpleStation14/Admin/Commands/slamofthenorthstar.ogg", Filter.Entities(comp.Owner), comp.Owner);
                _polymorphableSystem.PolymorphEntity(comp.Owner, "LMAObball");

                // var physics = _entities.GetComponent<PhysicsComponent>(comp.Owner);

                // var xform = Transform(comp.Owner);
                // var fixtures = _entities.GetComponent<FixturesComponent>(comp.Owner);
                // xform.Anchored = false; // Just in case.
                // physics.BodyType = BodyType.Dynamic;
                // physics.BodyStatus = BodyStatus.InAir;
                // physics.WakeBody();
                // foreach (var (_, fixture) in fixtures.Fixtures)
                // {
                //     if (!fixture.Hard)
                //         continue;
                //     fixture.Restitution = 1.1f;
                // }

                // physics.LinearVelocity = _random.NextVector2(1.5f, 1.5f);
                // physics.AngularVelocity = MathF.PI * 12;
                // physics.LinearDamping = 0.0f;
                // physics.AngularDamping = 0.0f;
            }
        }
    }
}
