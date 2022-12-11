using Content.Shared.Simplestation14.Nanites;
using Robust.Shared.Prototypes;

namespace Content.Server.SimpleStation14.Nanites
{
    public sealed class NaniteTemplateProgram : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        public override void Initialize()
        {
            base.Initialize();

            // (comment if not wanted)
            SubscribeLocalEvent<NaniteHostComponent, NaniteTrigger>(OnTrigger);
            SubscribeLocalEvent<NaniteHostComponent, NaniteProgramDeleted>(OnProgramDeleted);
        }

        // PASSIVE nanite functions (comment if not wanted, else shit will break)
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var Nanites in EntityQuery<NaniteHostComponent>())
            {
                // REPLACE regenAccumulator WITH YOUR ACCUMULATOR
                Nanites.regenAccumulator += frameTime;
                if (Nanites.regenAccumulator < 1f) continue;
                Nanites.regenAccumulator -= 1f;

                // Do something here every 1 second
            }
        }

        // TRIGGERED nanite functions (comment if not wanted)
        private void OnTrigger(EntityUid uid, NaniteHostComponent Nanites, NaniteTrigger args)
        {
            // Do something
        }

        private void OnProgramDeleted(EntityUid uid, NaniteHostComponent Nanites, NaniteProgramDeleted args)
        {
            var Program = args.Program;
            // Replace Program with your program Type
            if (Program.Type != "Program") return;

            // Do something
        }
    }
}
