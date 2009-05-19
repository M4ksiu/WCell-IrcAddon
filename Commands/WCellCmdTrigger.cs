using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCellStr = WCell.Util.StringStream;
using Squishy.Irc;

namespace WCell.IRCAddon.Commands
{
    public class WCellCmdTrigger : RealmServerCmdTrigger
    {
        public static IrcChannel Channel;
        public static IrcUser User;

        public WCellCmdTrigger(WCellUser user, IrcChannel channel, WCellStr text, RealmServerCmdArgs args) : base(text, args)
        {
        }

        public override void ReplyFormat(string txt)
        {
            Reply(ChatUtility.Strip(txt));
        }

        public override void Reply(string txt)
        {
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