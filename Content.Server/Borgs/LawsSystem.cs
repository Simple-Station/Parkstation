using Content.Shared.Borgs;
using Content.Server.Chat.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using JetBrains.Annotations;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Random;
using Content.Shared.SimpleStation14.Prototypes;
using Content.Shared.Random.Helpers;

namespace Content.Server.Borgs
{
    public sealed class LawsSystem : EntitySystem
    {
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<LawsComponent, StateLawsMessage>(OnStateLaws);
            SubscribeLocalEvent<LawsComponent, PlayerAttachedEvent>(OnPlayerAttached);

            SubscribeLocalEvent<LawsComponent, ComponentInit>(OnInit); // Parkstation-Laws
        }

        private void OnStateLaws(EntityUid uid, LawsComponent component, StateLawsMessage args)
        {
            StateLaws(uid, component);
        }

        private void OnPlayerAttached(EntityUid uid, LawsComponent component, PlayerAttachedEvent args)
        {
            if (!_uiSystem.TryGetUi(uid, LawsUiKey.Key, out var ui))
                return;

            _uiSystem.TryOpen(uid, LawsUiKey.Key, args.Player);
        }

        public void StateLaws(EntityUid uid, LawsComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            if (!component.CanState)
                return;

            if (component.StateTime != null && _timing.CurTime < component.StateTime)
                return;

            component.StateTime = _timing.CurTime + component.StateCD;

            foreach (var law in component.Laws)
            {
                _chat.TrySendInGameICMessage(uid, law, InGameICChatType.Speak, false);
            }
        }

        // Parkstation-Laws-Start

        private void OnInit(EntityUid uid, LawsComponent component, ComponentInit args)
        {
            if (component.LawsID == null)
                return;

            var _prototype = IoCManager.Resolve<IPrototypeManager>();

            if (_prototype.TryIndex<LawsPrototype>(component.LawsID, out var laws))
            {
                AddLawsFromPrototype(component, component.LawsID);

                return;
            }

            if (_prototype.TryIndex<WeightedRandomPrototype>(component.LawsID, out var collection))
            {
                AddLawsFromPrototype(component, collection.Pick());

                return;
            }

            Logger.Warning($"No laws prototype or weighted random found with ID {component.LawsID} for entity {ToPrettyString(uid)}.");
        }

        public void AddLawsFromPrototype(EntityUid uid, string prototypeID)
        {
            AddLawsFromPrototype(EnsureComp<LawsComponent>(uid), prototypeID);
        }

        public void AddLawsFromPrototype(LawsComponent component, string prototypeID)
        {
            var _prototype = IoCManager.Resolve<IPrototypeManager>();

            var laws = _prototype.Index<LawsPrototype>(prototypeID);

            component.Laws.Clear();

            // Add each law in the list.
            foreach (var law in laws.Laws)
            {
                component.Laws.Add(Loc.GetString(law));
            }

            Dirty(component);
        }

        // Parkstation-Laws-End

        [PublicAPI]
        public void ClearLaws(EntityUid uid, LawsComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return;

            component.Laws.Clear();
            Dirty(component);
        }

        public void AddLaw(EntityUid uid, string law, int? index = null, LawsComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return;

            if (index == null)
                index = component.Laws.Count;

            index = Math.Clamp((int) index, 0, component.Laws.Count);

            component.Laws.Insert((int) index, law);
            Dirty(component);
        }

        public void RemoveLaw(EntityUid uid, int? index = null, LawsComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return;

            if (index == null)
                index = component.Laws.Count;

            if (component.Laws.Count == 0)
                return;

            index = Math.Clamp((int) index, 0, component.Laws.Count - 1);

            if (index < 0)
                return;

            component.Laws.RemoveAt((int) index);
            Dirty(component);
        }
    }
}
