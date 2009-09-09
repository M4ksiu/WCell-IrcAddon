using System.Collections.Generic;
using Squishy.Irc;
using WCell.RealmServer;

namespace WCellAddon.IRCAddon
{
    public class IrcAddonConfig
    {
        public static Dictionary<string,string> ChannelList = new Dictionary<string, string>{{"#ChannelName", "ChannelKey"}};
                               //All the channels you want the client to connect to, key being the channel's key.
                               //If the key is empty (""), it will presume there is no key.
        public static string[] UpdatedChannels = {"#ChannelName"};
        public static string Info = "ChangeMe";
        public static string Network = "euroserv.fr.quakenet.org"; //The network the client will connect to.
        public static string[] Nicks = new[] { RealmServerConfiguration.RealmName, RealmServerConfiguration.RealmName + "2", RealmServerConfiguration.RealmName + "3"}; //The nicks the client will try and use.
        public static int Port = 6667; //The port the client will connect to.

        public static Privilege RequiredStaffPriv = Privilege.HalfOp;
        public static string UserName = "Mokbot";
    }
}