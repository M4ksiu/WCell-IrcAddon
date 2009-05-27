using Squishy.Irc;

namespace WCellAddon.IRCAddon
{
    public class IrcAddonConfig
    {
        public static string[] ChannelList = new[] {"#ChangeMe"};
                               //All the channels you want the client to connect to.

        public static string Info = "ChangeMe";
        public static string Network = "euroserv.fr.quakenet.org"; //The network the client will connect to.
        public static string[] Nicks = new[] {"Mokbot", "Mokbot2"}; //The nicks the client will try and use.
        public static int Port = 6667; //The port the client will connect to.

        public static Privilege RequiredStaffPriv = Privilege.HalfOp;
        public static string UserName = "Mokbot";
    }
}