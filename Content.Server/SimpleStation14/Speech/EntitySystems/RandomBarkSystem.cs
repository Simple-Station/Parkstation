using Content.Server.Chat.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Mind.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Server.GameObjects;

namespace Content.Server.SimpleStation14.Speech.RandomBark
{
    public sealed class RandomBarkSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly EntityManager _entities = default!;


        [ViewVariables(VVAccess.ReadWrite)]

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var barker in EntityQuery<RandomBarkComponent>())
            {
                barker.BarkAccumulator -= frameTime;
                if (barker.BarkAccumulator <= 0)
                {
                    barker.BarkAccumulator = _random.NextFloat(barker.MinTime, barker.MaxTime)*barker.BarkMultiplier;
                    if (_entities.TryGetComponent<ActorComponent>(barker.Owner, out var actComp))
                    {
                        return;
                    }
                    _chat.TrySendInGameICMessage(barker.Owner, _random.Pick(barker.Barks), InGameICChatType.Speak, !barker.Chatlog);
                }
            }
        }
        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
