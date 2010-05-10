using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCellStr = WCell.Util.Strings.StringStream;
using Squishy.Irc;

namespace IRCAddon.Commands
{
    public class WCellCmdTrigger : RealmServerCmdTrigger
    {
        public static string WCellCmdPrefix = "#";
        public IrcChannel Channel;
        public IrcUser User;

        public WCellCmdTrigger(WCellUser user, IrcChannel channel, WCellStr text, RealmServerCmdArgs args) : base(text, args)
        {
            Channel = channel;
            User = user.IrcUser;
        }

        public override void ReplyFormat(string txt)
        {
            Reply(ChatUtility.Strip(txt));
        }

        public override void Reply(string txt)
        {
			if(!IrcConnection.ReplyOnUnknownCommandUsed && txt.ToLower().Contains("unknown command"))
            {
                return;
            }

            if (Channel != null)
            {
                Channel.Msg(txt);
            }
            else
            {
                User.Msg(txt);
            }
        }
    }
}