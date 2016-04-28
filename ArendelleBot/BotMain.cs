using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using ChatSharp;
using ChatSharp.Events;
using System.Text.RegularExpressions;

namespace ArendelleBot {
    public class BotCore {
        private bool isRunning;
        private IrcClient client;
        private ProgramOptions options;
        private Dictionary<string, BotCommandInfo> commands;

        public BotCore(ProgramOptions opts) {
            options = opts;
            commands = new Dictionary<string, BotCommandInfo>();
            var addr = $"{opts.ServerAddr}:{opts.ServerPort}";
            client = new IrcClient(addr, new IrcUser(opts.Nickname, opts.Nickname));
            client.ConnectionComplete += OnConnectionComplete;
            client.ChannelMessageRecieved += OnChannelMessageReceived;
            client.PrivateMessageRecieved += OnPrivateMessageReceived;
            client.UserMessageRecieved += OnUserMessageReceived;
        }

        public void RegisterCommands<T>() where T : class {
			var all_mthd = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach(var mthd in all_mthd) {
                var attr = mthd.GetCustomAttribute<BotCommandAttribute>();
                if(attr == null) continue;
                var dlg = Delegate.CreateDelegate(typeof(BotCommandAction), mthd) as BotCommandAction;
                commands.Add(attr.Name, new BotCommandInfo() { Name = attr.Name, Action = dlg, Help = attr.Help });
            }
        }

        public void ExecuteCommand(string text, IrcUser user = null) {
            BotCommandInfo cmdinfo;
            var regex = new Regex(@"^(\S+)(.*)$").Match(text);
            string name = regex.Groups[1].Value,
                data = regex.Groups[2].Value.Trim();
            if(!commands.TryGetValue(name, out cmdinfo))
                throw new BotCommandException($"{Fmt.Color}04No such command: {name}{Fmt.Reset}");
            cmdinfo.Action(new BotCommandContext(this, client, user), data);
        }

        public bool GetCommand(string name, out BotCommandInfo info) {
            return commands.TryGetValue(name, out info);
        }

        public BotCommandInfo[] GetCommands() {
            return commands.Values.OrderBy(w => w.Name).ToArray();
        }

        public void ConnectAsync() {
            if(isRunning) return;
            isRunning = true;
            client.ConnectAsync();
            while(isRunning)
                Thread.Sleep(100);
        }

        public void Disconnect(string reason) {
            if(!isRunning) return;
            isRunning = false;
            client.Quit(reason);
        }

        private void OnConnectionComplete(object sender, EventArgs e) {
            foreach(var chan in options.Channels)
                if(chan.Length > 0)
                    client.JoinChannel(chan);
        }

        private void OnChannelMessageReceived(object sender, PrivateMessageEventArgs e) {
            // do stuff here
            var msg = e.PrivateMessage.Message;
            var chan = client.Channels[e.PrivateMessage.Source];
            { // URL Title Retrieval
                var regex = new Regex(@"\b(https?://\S+)\b").Matches(msg); // (?:\b|['""])
                foreach(Match match in regex) {
                    var url = match.Groups[1].Value;
                    Utils.GetHtmlTitleAsync(url,
                        v => client.SendMessage($"Found URL: {Fmt.Color}03{v}{Fmt.Reset}", chan.Name));
                }
            }
			{ // Russia!
				if (msg.ToLower().Contains("russia"))
					client.SendMessage($"{Fmt.Colors.Red}Russia!{Fmt.Reset}", chan.Name);
			}
        }

        private void OnPrivateMessageReceived(object sender, PrivateMessageEventArgs e) {
            if(e.PrivateMessage.IsChannelMessage) return;
            //var user = client.Users[e.PrivateMessage.Source];
            var user = e.PrivateMessage.User;
            var msg = e.PrivateMessage.Message;
            try {
                ExecuteCommand(msg, user);
            } catch(BotCommandException ex) {
                client.SendNotice($"Error ({ex.Message})", user.Nick);
            } catch(Exception ex) {
               Console.WriteLine($"CMD({msg}) ERR = {ex.Message}");
            }
        }
        
        private void OnUserMessageReceived(object sender, PrivateMessageEventArgs e) {
            // do stuff here
        }
    }
}
