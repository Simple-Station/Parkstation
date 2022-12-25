using Content.Shared.SimpleStation14.Nanites;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;

namespace Content.Server.SimpleStation14.Nanites
{
    public sealed class NaniteHostSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<NaniteHostComponent, ComponentStartup>(OnStartup);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var nanites in EntityQuery<NaniteHostComponent>())
            {
                nanites.regenAccumulator += frameTime;
                if (nanites.regenAccumulator < 1f) continue;
                else
                {
                    nanites.Nanites += nanites.RegenSpeed;
                }
                nanites.regenAccumulator -= 1f;

                nanites.syncAccumulator += frameTime;
                if (nanites.syncAccumulator < 10f) continue;
                else
                {
                    // Sync nanite programs with the cloud, CloudID
                }
                nanites.syncAccumulator -= 1f;
            }
        }

        private void OnStartup(EntityUid uid, NaniteHostComponent Nanite, ComponentStartup args)
        {
            if (Nanite.Nanites < Nanite.DefaultNanites) Nanite.Nanites = Nanite.DefaultNanites;

            NaniteProgram program1 = new()
            {
                EName = "Nanite Clot 1",
                EDescription = "Does something when tapped 1",
                Euid = 2,
                ETrigger = 0,
                Type = "Button",
            };
            NaniteProgram program2 = new()
            {
                EName = "Nanite Clot 2",
                EDescription = "Does something when tapped 2",
                Euid = 83,
                ETrigger = 1,
                Type = "Button",
            };
            NaniteProgram program3 = new()
            {
                EName = "Nanite Clot 3",
                EDescription = "Does something when tapped 3",
                Euid = 9213,
                ETrigger = 1,
                Type = "Dissolve",
            };

            Nanite.Programs.Add(program1);
            Nanite.Programs.Add(program2);
            Nanite.Programs.Add(program3);
        }
    }

    public abstract class NaniteTrigger : InstantActionEvent
    {
        public NaniteProgram Program = new();
        // public string Type = "Program";
        // public int ETrigger = 0;
        // public int Euid = 0;
    }

    public abstract class NaniteProgramDeleted : EntityEventArgs
    {
        public NaniteProgram Program = new();
    }
}
