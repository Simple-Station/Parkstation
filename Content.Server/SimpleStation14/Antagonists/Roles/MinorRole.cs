using Content.Server.Chat.Managers;
using Content.Server.Roles;
using Content.Shared.Roles;

namespace Content.Server.SimpleStation14.Flawed
{
    public sealed class FlawedRole : Role
    {
        public AntagPrototype Prototype { get; }

        public FlawedRole(Mind.Mind mind, AntagPrototype antagPrototype) : base(mind)
        {
            Prototype = antagPrototype;
            Name = antagPrototype.Name;
            Antagonist = antagPrototype.Antagonist;
        }

        public override string Name { get; }
        public override bool Antagonist { get; }

        public void GreetFlawed()
        {
            if (Mind.TryGetSession(out var session))
            {
                var chatMgr = IoCManager.Resolve<IChatManager>();
                chatMgr.DispatchServerMessage(session, Loc.GetString("flawed-role-greeting"));
            }
        }
    }
}
