using System.Collections.Generic;
using Squishy.Irc;
using WCell.RealmServer;
using WCell.Util.Variables;
using System;

namespace IRCAddon
{
	public class IrcAddonConfig
	{
        /// <summary>
        /// All the Channels you want the bot to join, must include the exception channel
        /// </summary>
        [Variable("Channels")]
		public static string[] ChannelListNames
        {
            get { return _channelListNames; }
			set
			{
                _channelListNames = value;
				ChannelList = new ChannelInfo[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					var chan = value[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					ChannelList[i] = new ChannelInfo
					{
						ChannelName = chan[0].Trim(),
						Password = (chan.Length > 1 && chan[1] != "ChannelKey") ? chan[1] : ""
					};
				}
			}
		}

		// Note: All the channels you want the client to connect to, key being the channel's key.
		// Note: If the key is empty (""), it will presume there is no key and connect normally
		// Note: Example 1) "#WCell," (will join the channel #WCell without a key)
		// NOTE: Example 2) "#WCell,password12" (will join the channel #WCell with the key "password12" (no spaces)

		private static string[] _channelListNames = { "#ChannelWithKey,ChannelKey", "#ChannelWithoutKey," };

	    /// <summary>
	    /// Channels you want to keep updated according to the latest server status
	    /// Broadcasts are also posted in these channels.
	    /// </summary>
	    [Variable("UpdatedChannels")] 
        public static string[] UpdatedChannelNames = new[] {"#ChannelName"};

        [NotVariable]
        public static ChannelInfo[] ChannelList = new ChannelInfo[0];

	    public static string ExceptionChan = "#ExceptionChannel";
		public static string Info = "ChangeMe";
		public static string Network = "euroserv.fr.quakenet.org"; //The network the client will connect to.
		public static string[] Nicks = new[] { RealmServerConfiguration.RealmName, RealmServerConfiguration.RealmName + "2", RealmServerConfiguration.RealmName + "3" }; //The nicks the client will try and use.
		public static int Port = 6667; //The port the client will connect to.

		public static Privilege RequiredStaffPriv = Privilege.HalfOp;
		public static string UserName = "Mokbot";
	}
}