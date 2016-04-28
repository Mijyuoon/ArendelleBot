using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using System.Net;
using ArendelleBot;

namespace ArendelleBot {
    public class ProgramOptions {
        public string ServerAddr = "127.0.0.1";
        public string ServerPort = "6667";
        public string Nickname = "ArendelleBot";
        public string[] Channels = new string[0];
    }
    
    class Program {
        static void Main(string[] args) {
            var opts = ParseOptions(args);
            var bot = new BotCore(opts);
            bot.RegisterModule<Cmd.Commands>();
            bot.RegisterModule<Cmd.Actions>();
            bot.ConnectAsync();
            Environment.Exit(0);
        }

        static ProgramOptions ParseOptions(string[] args) {
            var result = new ProgramOptions();
            bool display_help = false;
            var opts = new OptionSet() {
                { "a|addr=", "set IRC server address",
                    v => result.ServerAddr = v },
                { "p|port=", "set IRC server port",
                    v => result.ServerPort = v },
                { "n|nick=", "set bot nickname",
                    v => result.Nickname = v.Trim() },
                { "j|join=", "join specified channels",
                    v => result.Channels = v.Split(',') },
                { "h|help", "show this message and exit",
                    v => display_help = (v != null) },
            };
            try {
                opts.Parse(args);
                if(display_help) {
                    ShowHelp(opts);
                    Environment.Exit(0);
                }
                return result;
            } catch(Exception exc) {
                Console.WriteLine("ircbot: {0}", exc.Message);
                Console.WriteLine("Try 'ircbot --help' for more information");
                Environment.Exit(-1);
            }
            return null;
        }

        static void ShowHelp(OptionSet opts) {
            Console.WriteLine("Usage: ircbot <options>");
            Console.WriteLine("Mijyuoon's IRC bot software.");
            Console.WriteLine("\nOptions:");
            opts.WriteOptionDescriptions(Console.Out);
        }
    }
}
