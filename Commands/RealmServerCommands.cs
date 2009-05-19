using WCell.Constants.Factions;
using WCell.Core;
using WCell.RealmServer;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;


namespace WCell.IRCAddon.Commands
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
                    RealmAccount acc = ServerApp<RealmServer.RealmServer>.Instance.GetOrRequestAccount(accName);
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

        protected override void Initialize()
        {
            Init("??");
            EnglishDescription =
                "A compact help command to post only the essentials of each command. " +
                "When you have found your command, use Help or ? command for further information.";
            ParamInfo = "[CommandName]";
        }

        // IF there is something I'm not proud of it's this.
        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if(!trigger.Text.HasNext)
            {
                string shortCommandString = "Commands are: ";
                string formattedCmd;
                var cmds = CommandHandler.Instance.Commands;

                int i = 1;
                foreach(var cmd in cmds)
                {
                    formattedCmd = cmd.Name + ", ";
                    shortCommandString += formattedCmd;

                    i++;
                    if(i == 20)
                    {
                        i = 0;
                        shortCommandString += '%';
                    }
                }
                var cmdArray = shortCommandString.Split('%');
                foreach(var sentence in cmdArray)
                    trigger.Reply(sentence.TrimEnd(' ', ','));
            }
            else
            {
                string cmdStr = "";
                var cmdAlias = trigger.Text.Remainder;
                var cmds = CommandHandler.Instance.GetCommands(cmdAlias);
                string formattedString = "Found " + cmds.Count + " commands: ";
                var subCmdNames = " SubCommands: ";

                foreach(var cmd in cmds)
                {
                    cmdStr += "'" + cmd.Name + "' ,";

                    if(cmd.SubCommands.Count >= 1)
                    {
                        foreach (var subcmd in cmd.SubCommands)
                            subCmdNames += subcmd.Name + " ";

                        cmdStr += subCmdNames;
                    }
                }
                formattedString += cmdStr;
                if(cmds.Count > 0)
                    trigger.Reply(formattedString);
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
