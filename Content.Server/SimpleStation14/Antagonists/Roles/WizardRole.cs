using Content.Server.Chat.Managers;
using Content.Server.Roles;
using Content.Shared.Roles;

namespace Content.Server.SimpleStation14.Wizard
{
    public sealed class WizardRole : Role
    {
        public AntagPrototype Prototype { get; }

        public WizardRole(Mind.Mind mind, AntagPrototype antagPrototype) : base(mind)
        {
            Prototype = antagPrototype;
            Name = antagPrototype.Name;
            Antagonist = antagPrototype.Antagonist;
        }

        public override string Name { get; }
        public override bool Antagonist { get; }

        public void GreetWizard(string[] codewords)
        {
            if (Mind.TryGetSession(out var session))
            {
                var chatMgr = IoCManager.Resolve<IChatManager>();
                chatMgr.DispatchServerMessage(session, Loc.GetString("wizard-role-greeting"));
                chatMgr.DispatchServerMessage(session, Loc.GetString("wizard-role-codewords", ("codewords", string.Join(", ",codewords))));
            }
        }
    }
}
