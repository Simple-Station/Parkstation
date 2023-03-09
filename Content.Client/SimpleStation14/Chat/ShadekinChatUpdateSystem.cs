using Content.Client.Chat.Managers;
using Robust.Client.Player;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;

namespace Content.Client.SimpleStation14.Chat
{
    public sealed class ShadekinChatUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EmpathyChatComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<EmpathyChatComponent, ComponentRemove>(OnRemove);
        }

        public EmpathyChatComponent? Player => CompOrNull<EmpathyChatComponent>(_playerManager.LocalPlayer?.ControlledEntity);
        public bool IsShadekin => Player != null;

        private void OnInit(EntityUid uid, EmpathyChatComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, EmpathyChatComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}
