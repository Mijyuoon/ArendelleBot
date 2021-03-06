﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArendelleBot;

namespace ArendelleBot.Cmd {
	partial class Commands { 
        [BotCommand("JOIN", Help = "Joins specified channels")]
        static void JoinChannel(BotCommandContext ctx, string data) {
            foreach(var chan in Utils.SimpleParse(data))
                ctx.IRC.JoinChannel(chan);
        }

        [BotCommand("HELP", Help = "Lists all commands / shows help for a command")]
        static void ShowHelp(BotCommandContext ctx, string data) {
            if(data.Length > 0) {
                BotCommandInfo info;
                if(ctx.Core.GetCommand(data, out info)) {
                    var helpStr = info.ExtHelp ?? info.Help;
                    ctx.IRC.SendMultilineNotice(helpStr, false, ctx.User.Nick);
                } else {
                    ctx.IRC.SendNotice($"No help available for {Fmt.Bold}{data}{Fmt.Reset}", ctx.User.Nick);
                }
            } else {
                ctx.IRC.SendNotice("Available commands:", ctx.User.Nick);
                foreach(var cmd in ctx.Core.GetCommands()) {
                    ctx.IRC.SendNotice($"    {Fmt.Bold}{cmd.Name,-15}{Fmt.Reset} {cmd.Help}", ctx.User.Nick);
                }
            }
        }

        [BotCommand("SAY", Help = "Sends a message on specified channels")]
        static void SendMsg(BotCommandContext ctx, string data) {
            var args = Utils.SimpleParse(data);
            var text = string.Join(" ", args.Skip(1)).Trim();
            var msg = $"<{Fmt.Colorize(Fmt.Colors.Teal)}{ctx.User.Nick}{Fmt.Reset}> {text}";

            switch(args[0]) {
            case "ALL": {
                var chans = ctx.IRC.Channels.Select(w => w.Name).ToArray();
                ctx.IRC.SendMessage(msg, chans);
                break; }
            default: {
                var chans = args[0].Split(',', ' ');
                ctx.IRC.SendMessage(msg, chans);
                break; }
            }
        }

        [BotCommand("RAWCMD", Help = "Sends a raw IRC message (use with caution)")]
        static void SendRaw(BotCommandContext ctx, string data) {
            ctx.IRC.SendRawMessage("{0}", data);
        }

        [BotCommand("QUIT", Help = "Disconnects and shuts down the bot")]
        static void QuitBot(BotCommandContext ctx, string data) {
            ctx.Core.Disconnect(data);
        }
    }

    partial class Actions {
        [BotAction]
        static void HtmlTitles(BotMessageContext ctx, string data) {
            var chan = ctx.IRC.Channels[ctx.Msg.Source];
            var matches = new Regex(@"\b(https?://\S+)\b").Matches(data); // (?:\b|['""])
            foreach(Match match in matches) {
                var url = match.Groups[1].Value;
                Utils.GetHtmlTitleAsync(url,
                    v => ctx.IRC.SendMessage($"Found URL: {Fmt.Colorize(Fmt.Colors.Green)}{v}{Fmt.Reset}", chan.Name));
            }
        }

        [BotAction]
        static void TheRussia(BotMessageContext ctx, string data) {
            var chan = ctx.IRC.Channels[ctx.Msg.Source];
            var regex = new Regex(@"\brussia(?:|ns?)\b", RegexOptions.IgnoreCase);
            if(regex.Match(data).Success)
                ctx.IRC.SendMessage($"{Fmt.Colorize(Fmt.Colors.Red)}Russia!{Fmt.Reset}", chan.Name);
        }
    }
}