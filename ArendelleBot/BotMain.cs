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
        private List<BotMessageAction> msgActions;

        public BotCore(ProgramOptions opts) {
            options = opts;
            commands = new Dictionary<string, BotCommandInfo>();
            msgActions = new List<BotMessageAction>();
            var addr = $"{opts.ServerAddr}:{opts.ServerPort}";
            client = new IrcClient(addr, new IrcUser(opts.Nickname, opts.Nickname));
            client.ConnectionComplete += OnConnectionComplete;
            client.ChannelMessageRecieved += OnChannelMessageReceived;
            client.PrivateMessageRecieved += OnPrivateMessageReceived;
            client.UserMessageRecieved += OnUserMessageReceived;
        }

        public void RegisterModule<T>() where T : class {
            var all_mthd = typeof(T).GetMethods(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
            foreach(var mthd in all_mthd) {
                var cAttr = mthd.GetCustomAttribute<BotCommandAttribute>();
                if(cAttr != null) {
                    var dlg = Delegate.CreateDelegate(typeof(BotCommandAction), mthd) as BotCommandAction;
                    commands.Add(cAttr.Name, new BotCommandInfo() { Name = cAttr.Name, Action = dlg, Help = cAttr.Help });
                }

                var mAttr = mthd.GetCustomAttribute<BotActionAttribute>();
                if(mAttr != null) {
                    var dlg = Delegate.CreateDelegate(typeof(BotMessageAction), mthd) as BotMessageAction;
                    msgActions.Add(dlg);
                }
            }
        }

        public void ExecuteCommand(string text, IrcUser user = null) {
            BotCommandInfo cmdinfo;
            var regex = new Regex(@"^(\S+)(.*)$").Match(text);
            string name = regex.Groups[1].Value,
                data = regex.Groups[2].Value.Trim();
            if(!commands.TryGetValue(name, out cmdinfo))
                throw new BotCommandException($"{Fmt.Colorize(Fmt.Colors.Red)}No such command: {name}{Fmt.Reset}");
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
            foreach(var action in msgActions) {
                action(new BotMessageContext(this, client, e.PrivateMessage), e.PrivateMessage.Message);
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
