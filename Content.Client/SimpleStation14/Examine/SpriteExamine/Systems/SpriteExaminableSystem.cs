using Content.Client.Examine;
using Content.Client.Inventory;
using Content.Shared.SimpleStation14.Examine.SpriteExamine.Components;
using Content.Client.SimpleStation14.Examine.SpriteExamine.UI;
using Content.Shared.Access.Components;
using Content.Shared.Chat;
using Content.Shared.DetailExaminable;
using Content.Shared.PDA;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Client.SimpleStation14.Examine.SpriteExamine.Systems;

public sealed class SpriteExaminableSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly ClientInventorySystem _inventory = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    private SpriteExamineWindow? _window;


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
                Do(args.Target);
            },
            Text = Loc.GetString("character-sprite-examine-verb-text"),
            Message = Loc.GetString("character-sprite-examine-verb-message"),
            Category = VerbCategory.Examine,
            Disabled = !_examine.IsInDetailsRange(args.User, uid),
            Icon = new SpriteSpecifier.Texture(new ResourcePath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")),
            ClientExclusive = true,
        };

        args.Verbs.Add(verb);
    }


    private void Do(EntityUid uid)
    {
        if (_window == null)
            return;

        string? name = null;
        string? job = null;
        string? flavorText = null;

        if (_inventory.TryGetSlotEntity(uid, "id", out var idUid))
        {
            var id = GetId(idUid);
            if (id is not null)
            {
                var info = GetNameAndJob(id);

                name = info.Item1;
                job = info.Item2;
            }
        }

        if (_entity.TryGetComponent<DetailExaminableComponent>(uid, out var detail))
        {
            flavorText = detail.Content;
        }

        _window.UpdateUi(uid, name, job, flavorText);
        _window.OpenCentered();
    }

    /// <summary>
    ///     Gets the ID card component from either a PDA or an ID card if the entity has one
    /// </summary>
    /// <param name="idUid">Entity to check</param>
    /// <returns>ID card component if they have one on the entity</returns>
    private IdCardComponent? GetId(EntityUid? idUid)
    {
        // PDA
        if (_entity.TryGetComponent(idUid, out PDAComponent? pda) && pda.ContainedID is not null)
            return pda.ContainedID;
        // ID Card
        if (_entity.TryGetComponent(idUid, out IdCardComponent? id))
            return id;

        return null;
    }

    /// <summary>
    ///     Gets the name and job title from an ID card component
    /// </summary>
    /// <param name="id">The ID card to retrieve the information from</param>
    /// <returns>Name, Job Title</returns>
    private (string, string) GetNameAndJob(IdCardComponent id)
    {
        var name = id.FullName;
        if (string.IsNullOrEmpty(name))
            name = "Unknown";

        var jobTitle = id.JobTitle;
        if (string.IsNullOrEmpty(jobTitle))
            jobTitle = "Unknown";
        jobTitle = _chat.SanitizeMessageCapital(jobTitle);

        return (name, jobTitle);
    }
}
