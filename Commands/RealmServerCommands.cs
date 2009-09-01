using Squishy.Irc;
using Squishy.Irc.Protocol;
using WCell.Constants.Factions;
using WCell.Core;
using WCell.RealmServer;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;


namespace WCellAddon.IRCAddon.Commands
{
    #region Custom RealmServerCommands

    #region CheckBanCommands

    public class CheckBanCommand : RealmServerCommand
    {
        protected CheckBanCommand()
        {
        }

        protected override void Initialize()
        {
            Init("IsBanned", "CheckBan", "CB");
            EnglishDescription = "Checks whether an account/character/ip is banned";
        }

        #region Nested type: CheckAccountBanCommand

        public class CheckAccountBanCommand : SubCommand
        {
            protected CheckAccountBanCommand()
            {
            }

            protected override void Initialize()
            {
                Init("Account", "Acc", "A");
                EnglishDescription = "Checks for an account ban";
                ParamInfo = "<Account>";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string accName = trigger.Text.NextWord();
                if (accName != null)
                {
                    RealmAccount acc = ServerApp<RealmServer>.Instance.GetOrRequestAccount(accName);
                    if (acc != null)
                    {
                        if (acc.IsActive)
                        {
                            trigger.Reply("Account '" + acc.Name + "' is not banned.");
                        }

                        else
                        {
                            trigger.Reply(acc.StatusUntil.Value.ToLongDateString());
                        }
                    }

                    else
                    {
                        trigger.Reply("Account '" + accName + "' does not exist.");
                    }
                }
                else
                {
                    trigger.Reply("Please include the account name.");
                }
            }
        }

        #endregion

        #region Nested type: CheckIPBanCommand

        public class CheckIPBanCommand : SubCommand
        {

            protected CheckIPBanCommand()
            {
            }

            protected override void Initialize()
            {
                Init("IP");
                ParamInfo = "<IP>";
                EnglishDescription = "Checks for an ip ban";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("Sorry, someone forgot to add IP banning via RealmServer to WCell. Will be implemented soon :P");
                //TODO: Finish this when someone implements it
            }
        }

        #endregion

        #region Nested type: CheckCharBanCommand

        public class CheckCharBanCommand : SubCommand
        {
            protected CheckCharBanCommand()
            {
            }

            protected override void Initialize()
            {
                Init("Char", "Chr", "C");
                ParamInfo = "<CharacterName>";
                EnglishDescription = "Checks for a character ban";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var name = trigger.Text.NextWord();
                var chr = trigger.Args.GetCharArgumentOrTarget(trigger, name);

                //CharacterRecord.TryFind(chr.EntityId).
                trigger.Reply("Sorry, someone forgot to add char banning to WCell. Will be implemented soon :P");
                //TODO: Finish this when someone implements it
            }
        }

        #endregion
    }

    #endregion

    #region CompactHelpCommand

    public class CompactHelpCommand : RealmServerCommand
    {
        public static int MaxUncompressedCmds = 3;

        protected override void Initialize()
        {
            Init("??");
            EnglishDescription =
                "A compact help command to post only the essentials of each command. " +
                "When you have found your command, use Help or ? command for further information.";
            ParamInfo = "[CommandName]";
        }

        // Domi's helpcommand just modified.
        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string alias = trigger.Text.NextWord();
            var cmds = RealmCommandHandler.Instance.Commands;

            if(alias.Length > 0)
            {
                var matches = RealmCommandHandler.Instance.GetCommands(alias);
                if(matches == null)
                {
                    trigger.Reply("Command '{0}' does not exist", alias);
                    return;
                }

                else if (matches.Count <= MaxUncompressedCmds)
                {
                    foreach(var cmd in matches)
                    {
                        string desc = string.Format("{0} ({1})", cmd.Usage, cmd.EnglishDescription);
                        trigger.Reply(desc);
                    }
                }
                return;
            }
            trigger.Reply("All current commands:");
            string line = "";
            foreach (var cmd in cmds)
            {
                if (cmd.Enabled && cmd.MayTrigger(trigger, cmd, false))
                {
                    string subCmdStr = "";
                    foreach(var subCmd in cmd.SubCommands)
                    {
                        subCmdStr += subCmd.Name + ", ";
                    }
                    var cmdStr = cmd.Name + " (" + cmd.Usage + ") ";

                    if (line.Length + cmdStr.Length >= IrcProtocol.MaxModCount)
                    {
                        trigger.Reply(line);
                        line = "";
                    }
                    else
                    {
                        line += " ";
                    }

                    line += cmdStr;
                }
            }

            if (line.Length > 0)
            {
                trigger.Reply(line);
            }
        }
    }

    #endregion

    #region AddTestChannel

    public class AddTestChannelCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("ADT");
            Enabled = true;
            EnglishDescription = "Command to create a new channel, make the WCellUser join and then echo the chat";
            ParamInfo = "<ChannelName> <Player>";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var wcellUser = trigger.Args.User as WCellUser;
            var chanName = trigger.Text.NextWord();
            var targetUsr = trigger.Args.GetCharArgumentOrTarget(trigger, trigger.Text.NextWord());

            ChannelMember me = new ChannelMember(wcellUser);
            ChatChannelGroup ircChannelGroup = new ChatChannelGroup(FactionGroup.Invalid);
            var chatChan = new ChatChannel(ircChannelGroup, chanName);
            chatChan.Invite(me, targetUsr);
            //ChatMgr.OnChat += new ChatNotifyDelegate(ChatMgr_OnChat);

        }

        //void ChatMgr_OnChat(IChatter chatter, string message, WCell.Constants.ChatLanguage lang, WCell.Constants.ChatMsgType chatType, IGenericChatTarget target)
        //{
        //    throw new System.NotImplementedException();
        //}
    }

    #endregion

    #endregion
}
