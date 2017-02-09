using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

using Mono.Data.Sqlite;
using Mono.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Chat_Tag_Blocker_v2 {
    [ApiVersion(2, 0)]
    public class ChatBlocker : TerrariaPlugin {
        public override string Name { get { return "Tag Blocker v2"; } }
        public override string Description { get { return "Locks all of Terraria's Chat Tags and adds a Regex Chat Filter."; } }
        public override string Author { get { return "xCykrix"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        private SqliteConnection ac_db;
        private List<String> BlacklistedWords = new List<String>();

        public ChatBlocker(Main game) : base(game) { }

        public override void Initialize()
        {
            ac_db = OpenDB("ChatFilter.sqlite", "TagBlockerFilter");
            RunVoidCommand(ac_db, "CREATE TABLE blacklist ( word nvarchar(32) );");
            UpdateDefinitions(ac_db, "SELECT * FROM blacklist;");
            ServerApi.Hooks.ServerChat.Register(this, ChatSent, 10);
            ServerApi.Hooks.ServerChat.Register(this, ChatFilterProcessSent, 11);
            ServerApi.Hooks.ServerJoin.Register(this, PlayerJoin, 10);
            GeneralHooks.ReloadEvent += ReloadSent;
            TShock.Log.Info("Definitions: " + BlacklistedWords.ToString());
        }

        private void ReloadSent(ReloadEventArgs args)
        {
            UpdateDefinitions(ac_db, "SELECT * FROM BLACKLIST");
            args.Player.SendSuccessMessage("Updated ChatManager Word Blacklist Definitions.");
        }

        private SqliteConnection OpenDB(String FileName, String SubDirectory)
        {
            try
            {
                // Path and Directory
                String path = Path.Combine(TShock.SavePath, SubDirectory);
                bool exists = System.IO.Directory.Exists(path);
                if (!exists) System.IO.Directory.CreateDirectory(path);

                // Connection
                String sql = Path.Combine(TShock.SavePath, SubDirectory, FileName);
                SqliteConnection sqConn;
                sqConn = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                return sqConn;
            }
            catch (Exception x)
            {
                TShock.Log.Error(x.ToString());
                return null;
            }
        }

        private void UpdateDefinitions(SqliteConnection conn, String command)
        {
            conn.Open();
            SqliteCommand sqComm = conn.CreateCommand();
            sqComm.CommandText = command;
            SqliteDataReader sqReader = sqComm.ExecuteReader();
            BlacklistedWords.Clear();
            while (sqReader.Read())
            {
                String word = sqReader.GetString(0);
                BlacklistedWords.Add(word);
            }
            TShock.Log.ConsoleInfo("[TagBlocker] Updated ChatManager Word Blacklist Definitions.");
            conn.Close();
        }

        private void RunVoidCommand(SqliteConnection conn, String command)
        {
            conn.Open();
            try
            {
                SqliteCommand sqComm = conn.CreateCommand();
                sqComm.CommandText = command;
                SqliteDataReader sqReader = sqComm.ExecuteReader();
            }
            catch (Exception x)
            {
                TShock.Log.ConsoleInfo("[TagBlocker] Failed to run SQL Command.");
            }
            conn.Close();
        }

        private void ChatFilterProcessSent(TerrariaApi.Server.ServerChatEventArgs args)
        {
            TSPlayer target = TShock.Players[args.Who];
            String message = args.Text.ToLower();
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            String result = rgx.Replace(message, "");

            if (!target.HasPermission("tagblock.filter.ignore"))
            {
                bool blockMessage = false;
                BlacklistedWords.ForEach(delegate (String possibleBlock)
                {
                    if (result.Contains(possibleBlock))
                    {
                        blockMessage = true;
                    }
                });

                if (blockMessage)
                {
                    target.SendWarningMessage("[TagBlock-ChatFilter] You are not permitted to use one of these words. (Detected attempt of using banned word<s>.)");
                    args.Handled = true;
                }
            };
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

        private static List<Match> ShowMatch(string text, string expr)
        {
            MatchCollection mc = Regex.Matches(text, expr);
            List<Match> matches = new List<Match>();
            foreach (Match m in mc)
            {
                matches.Add(m);
            }
            return matches;
        }

    }
}
