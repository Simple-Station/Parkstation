using Content.Client.Chat.Managers;
using Robust.Client.Player;
using Content.Shared.SimpleStation14.Magic.Components;

namespace Content.Client.SimpleStation14.Chat
{
    public sealed class ShadekinChatUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShadekinComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ShadekinComponent, ComponentRemove>(OnRemove);
        }

        public ShadekinComponent? Player => CompOrNull<ShadekinComponent>(_playerManager.LocalPlayer?.ControlledEntity);
        public bool IsShadekin => Player != null;

        private void OnInit(EntityUid uid, ShadekinComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, ShadekinComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}
