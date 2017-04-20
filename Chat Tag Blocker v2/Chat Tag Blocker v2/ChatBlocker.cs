using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Chat_Tag_Blocker_v2 {
    [ApiVersion(2, 0)]
    public class ChatBlocker : TerrariaPlugin {
        public override string Name { get { return "Chat Tag Blocker v2"; } }
        public override string Description { get { return "Allows blocking Chat, Item, User, and Achievement chat tags."; } }
        public override string Author { get { return "xCykrix"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public ChatBlocker(Main game) : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, ChatSent);
            ServerApi.Hooks.ServerJoin.Register(this, PlayerJoin);
        }

        private void ChatSent(TerrariaApi.Server.ServerChatEventArgs args)
        {
            TSPlayer target = TShock.Players[args.Who];
            String message = args.Text.ToLower();

            if (!target.HasPermission("tagblock.allow.text.color"))
            {
                if (message.Contains("[c:") || message.Contains("[c/") && message.Contains("]"))
                {
                    target.SendWarningMessage("[TagBlock] You are not permitted to use the Color Tag. (Detected attempt.)");
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.text.item"))
            {
                if (message.Contains("[i:") || message.Contains("[i/") && message.Contains("]"))
                {
                    target.SendWarningMessage("[TagBlock] You are not permitted to use the Item Tag. (Detected attempt.)");
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.text.achievement"))
            {
                if (message.Contains("[a:") || message.Contains("[a/") && message.Contains("]"))
                {
                    target.SendWarningMessage("[TagBlock] You are not permitted to use the Achievement Tag. (Detected attempt.)");
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.text.player"))
            {
                if (message.Contains("[n:") || message.Contains("[n/") && message.Contains("]"))
                {
                    target.SendWarningMessage("[TagBlock] You are not permitted to use the Player Name Tag. (Detected attempt.)");
                    args.Handled = true;
                }
            };
        }

        private void PlayerJoin(TerrariaApi.Server.JoinEventArgs args)
        {
            TSPlayer target = TShock.Players[args.Who];
            String message = target.Name;

            if (!target.HasPermission("tagblock.allow.name.color"))
            {
                if (message.Contains("[c:") || message.Contains("[c/") && message.Contains("]"))
                {
                    TShock.Utils.ForceKick(target, "Please remove the Color Tag from your username.\n  You can a character editor to do this.", true, true);
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.name.item"))
            {
                if (message.Contains("[i:") || message.Contains("[i/") && message.Contains("]"))
                {
                    TShock.Utils.ForceKick(target, "Please remove the Item Tag from your username.\n  You can a character editor to do this.", true, true);
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.name.achievement"))
            {
                if (message.Contains("[a:") || message.Contains("[a/") && message.Contains("]"))
                {
                    TShock.Utils.ForceKick(target, "Please remove the Achievement Tag from your username.\n  You can a character editor to do this.", true, true);
                    args.Handled = true;
                }
            };

            if (!target.HasPermission("tagblock.allow.name.player"))
            {
                if (message.Contains("[n:") || message.Contains("[n/") && message.Contains("]"))
                {
                    TShock.Utils.ForceKick(target, "Please remove the Player Name Tag from your username.\n  You can a character editor to do this.", true, true);
                    args.Handled = true;
                }
            };
        }

    }
}
