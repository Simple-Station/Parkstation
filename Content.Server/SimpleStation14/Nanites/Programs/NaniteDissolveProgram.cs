// This is just for testing the events actually work until I get a proper program
using Content.Server.SimpleStation14.Nanites;
using Content.Shared.SimpleStation14.Nanites;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;

namespace Content.Server.SimpleStation14.Nanites
{
    public sealed class NaniteDissolveProgram : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<NaniteHostComponent, NaniteDissolveTrigger>(OnTrigger);
            SubscribeLocalEvent<NaniteHostComponent, NaniteDissolveProgramDeleted>(OnProgramDeleted);
        }

        public void OnTrigger(EntityUid uid, NaniteHostComponent Nanites, NaniteDissolveTrigger args)
        {
            foreach (var Program in Nanites.Programs)
            {
                Console.WriteLine("aETrigger", args.Program.ETrigger);
                Console.WriteLine("aType", args.Program.Type);
                Console.WriteLine("pETrigger", Program.ETrigger);
                Console.WriteLine("pType", Program.Type);

                if (Program.Type != args.Program.Type) continue;
                if (args.Program.ETrigger != Program.ETrigger) continue;

                if (Nanites.Nanites - 5 >= Nanites.SafetyLevel) Nanites.Nanites -= 5;
            }
        }

        public void OnProgramDeleted(EntityUid uid, NaniteHostComponent Nanites, NaniteDissolveProgramDeleted args)
        {
            var Program = args.Program;
            if (Program.Type != "Dissolve") return;

            // Do something
        }
    }

    public sealed class NaniteDissolveTrigger : NaniteTrigger { }
    public sealed class NaniteDissolveProgramDeleted : NaniteProgramDeleted { }
}
