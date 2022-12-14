using Content.Shared.SimpleStation14.Nanites;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;

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
            SubscribeLocalEvent<NaniteHostComponent, NaniteTemplateTrigger>(OnTrigger);
            SubscribeLocalEvent<NaniteHostComponent, NaniteTemplateProgramDeleted>(OnProgramDeleted);
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
        public void OnTrigger(EntityUid uid, NaniteHostComponent Nanites, NaniteTemplateTrigger args)
        {
            // Do something
        }

        public void OnProgramDeleted(EntityUid uid, NaniteHostComponent Nanites, NaniteTemplateProgramDeleted args)
        {
            var Program = args.Program;
            // Replace Program with your program Type
            if (Program.Type != "Program") return;

            // Do something
        }
    }

    // Replace Template with program Type
    public sealed class NaniteTemplateTrigger : InstantActionEvent
    {
        public int ETrigger = 0;
    }
    // Replace Template with program Type
    public sealed class NaniteTemplateProgramDeleted : EntityEventArgs
    {
        public NaniteProgram Program = new();
    }
}
