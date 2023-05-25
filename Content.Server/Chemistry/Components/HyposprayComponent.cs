using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server.Chemistry.Components
{
    [RegisterComponent]
    public sealed class HyposprayComponent : SharedHyposprayComponent
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        [DataField("clumsyFailChance")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float ClumsyFailChance { get; set; } = 0.5f;

        [DataField("transferAmount")]
        [ViewVariables(VVAccess.ReadWrite)]
        public FixedPoint2 TransferAmount { get; set; } = FixedPoint2.New(5);

        [DataField("injectSound")]
        public SoundSpecifier InjectSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");

        /// <summary>
        ///     Whether the hypospray uses a needle (i.e. medipens)
        ///     or sci fi bullshit that sprays into the bloodstream directly (i.e. hypos)
        /// </summary>
        [DataField("pierceArmor")]
        public bool PierceArmor = false;

        public override ComponentState GetComponentState()
        {
            var itemSlotSys = _entMan.EntitySysManager.GetEntitySystem<ItemSlotsSystem>();
            var solutionSys = _entMan.EntitySysManager.GetEntitySystem<SolutionContainerSystem>();

            EntityUid? container = Owner;
            if (SolutionSlot != null) {
                container =  itemSlotSys.GetItemOrNull(Owner, SolutionSlot);
            }
            return solutionSys.TryGetSolution(container, SolutionName, out var solution)
                ? new HyposprayComponentState(solution.Volume, solution.MaxVolume)
                : new HyposprayComponentState(FixedPoint2.Zero, FixedPoint2.Zero);
        }
    }
}
