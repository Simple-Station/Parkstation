using Content.Shared.SimpleStation14.Examine.SpriteExamine.Components;
using Content.Client.SimpleStation14.Examine.SpriteExamine.UI;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.SimpleStation14.Examine.SpriteExamine.Systems
{
    public sealed class SpriteExamineableSystem : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        SpriteExamineWindow? _window;

        public override void Initialize()
        {
            base.Initialize();

            _window = new SpriteExamineWindow();

            SubscribeLocalEvent<SpriteExaminableComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
        }


        private void OnGetExamineVerbs(EntityUid uid, SpriteExaminableComponent component, GetVerbsEvent<ExamineVerb> args)
        {
            var verb = new ExamineVerb()
            {
                Act = () =>
                {
                    if (_window == null) return;
                    _window.UpdateUi(uid);
                    _window.OpenCentered();
                },
                Text = Loc.GetString("character-sprite-examine-verb-text"),
                Message = Loc.GetString("character-sprite-examine-verb-message"),
                Category = VerbCategory.Examine,
                Icon = new SpriteSpecifier.Texture(new ResourcePath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")),
                ClientExclusive = true,
            };

            args.Verbs.Add(verb);
        }
    }
}
