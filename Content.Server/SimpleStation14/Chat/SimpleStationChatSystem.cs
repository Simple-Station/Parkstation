using System.Linq;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Bed.Sleep;
using Content.Shared.Drugs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Psionics.Glimmer;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Shared.SimpleStation14.Species.Shadekin.Components;
using Robust.Shared.Utility;
using Content.Server.SimpleStation14.Speech.EntitySystems;

namespace Content.Server.SimpleStation14.Chat
{
    /// <summary>
    /// Extensions for parkstation's chat stuff
    /// </summary>
    public sealed class SimpleStationChatSystem : EntitySystem
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedGlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;

        private IEnumerable<INetChannel> GetShadekinChatClients()
        {
            return Filter.Empty()
                .AddWhereAttachedEntity(entity => HasComp<ShadekinComponent>(entity))
                .Recipients
                .Select(p => p.ConnectedClient);
        }

        private IEnumerable<INetChannel> GetAdminClients()
        {
            return _adminManager.ActiveAdmins
                .Select(p => p.ConnectedClient);
        }

        public void SendEmpathyChat(EntityUid source, string message, bool hideChat)
        {
            if (!HasComp<ShadekinComponent>(source)) return;

            var clients = GetShadekinChatClients();
            var admins = GetAdminClients();
            string localMessage = EntitySystem.Get<ShadekinAccentSystem>().Accentuate(message);
            string localMessageWrap;
            string messageWrap;
            string adminMessageWrap;

            localMessageWrap = Loc.GetString("chat-manager-entity-say-wrap-message", ("entityName", source), ("message", FormattedMessage.EscapeText(message)));
            messageWrap = Loc.GetString("chat-manager-send-empathy-chat-wrap-message", ("empathyChannelName", Loc.GetString("chat-manager-empathy-channel-name")),("source", source), ("message", message));
            adminMessageWrap = Loc.GetString("chat-manager-send-empathy-chat-wrap-message", ("empathyChannelName", Loc.GetString("chat-manager-empathy-channel-name")),("source", source), ("message", message));

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Empathy chat from {ToPrettyString(source):Player}: {message}");

            _chatSystem.TrySendInGameICMessage(source, localMessage, InGameICChatType.Speak, hideChat);
            _chatManager.ChatMessageToMany(ChatChannel.Empathy, message, messageWrap, source, hideChat, true, clients.ToList(), Color.MediumPurple);
            _chatManager.ChatMessageToMany(ChatChannel.Empathy, message, adminMessageWrap, source, hideChat, true, admins, Color.MediumPurple);
        }
    }
}
