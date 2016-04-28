using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatSharp;

namespace ArendelleBot {
    [AttributeUsage(AttributeTargets.Method)]
    public class BotCommandAttribute : Attribute {
        public string Name { get; private set; }
        public string Help { get; set; }

        public BotCommandAttribute(string Name) {
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class BotActionAttribute : Attribute {
        public BotActionAttribute() {
            
        }
    }

    public struct BotCommandContext {
        public BotCore Core { get; private set; }
        public IrcClient IRC { get; private set; }
        public IrcUser User { get; private set; }

        public BotCommandContext(BotCore core, IrcClient irc, IrcUser user) {
            Core = core; IRC = irc; User = user;
        }
    }

    public delegate void BotCommandAction(BotCommandContext ctx, string data);

    public struct BotCommandInfo {
        public string Name { get; set; }
        public BotCommandAction Action { get; set; }
        public string Help { get; set; }
        public string ExtHelp { get; set; }
    }

    public struct BotMessageContext {
        public BotCore Core { get; private set; }
        public IrcClient IRC { get; private set; }
        public PrivateMessage Msg { get; private set; }

        public BotMessageContext(BotCore core, IrcClient irc, PrivateMessage msg) {
            Core = core; IRC = irc; Msg = msg;
        }
    }

    public delegate void BotMessageAction(BotMessageContext ctx, string data);

    class BotCommandException : Exception {
        public BotCommandException(string message) : base(message) {}
    }
}
