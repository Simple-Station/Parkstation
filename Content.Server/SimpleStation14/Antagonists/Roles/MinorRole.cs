using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.Roles;

namespace Content.Server.SimpleStation14.Minor
{
    public sealed class MinorRole : Role
    {
        public AntagPrototype Prototype { get; }

        public MinorRole(Mind.Mind mind, AntagPrototype antagPrototype) : base(mind)
        {
            Prototype = antagPrototype;
            Name = antagPrototype.Name;
            Antagonist = antagPrototype.Antagonist;
        }

        public override string Name { get; }
        public override bool Antagonist { get; }

        public void GreetMinor()
        {
            if (!IoCManager.Resolve<MindSystem>().TryGetSession(Mind, out var session))
                return;

            var chatMgr = IoCManager.Resolve<IChatManager>();
            chatMgr.DispatchServerMessage(session, Loc.GetString("minor-role-greeting"));
        }
    }
}
