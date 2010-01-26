using System.Collections.Generic;
using Squishy.Irc;
using WCell.RealmServer;
using WCell.Util.Variables;
using System;

namespace IRCAddon
{
	public class IrcAddonConfig
	{
		public static string[] ChannelList = { "#ChannelName,ChannelKey" };
		// Note: All the channels you want the client to connect to, key being the channel's key.
		// Note: If the key is empty (""), it will presume there is no key and connect normally
		// Note: Example 1) "#WCell," (will join the channel #WCell without a key)
		// NOTE: Example 2) "#WCell,password12" (will join the channel #WCell with the key "password12" (no spaces)

		private static string[] updatedChannelNames = { "#ChannelName" };

		/// <summary>
		/// Channels you want to keep updated according to the latest server status
		/// Broadcasts are also posted in these channels.
		/// </summary>
		[Variable("UpdatedChannels")]
		public static string[] UpdatedChannelNames
		{
			get { return updatedChannelNames; }
			set
			{
				updatedChannelNames = value;
				UpdatedChannels = new ChannelInfo[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					var chan = value[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					UpdatedChannels[i] = new ChannelInfo
					{
						ChannelName = chan[0].Trim(),
						Password = chan.Length > 1 ? chan[1] : ""
					};
				}
			}
		}

		public static ChannelInfo[] UpdatedChannels = new ChannelInfo[0];

		public static string[] ExceptionChan = { "#ExceptionChannel,ChannelKey" };
		public static string Info = "ChangeMe";
		public static string Network = "euroserv.fr.quakenet.org"; //The network the client will connect to.
		public static string[] Nicks = new[] { RealmServerConfiguration.RealmName, RealmServerConfiguration.RealmName + "2", RealmServerConfiguration.RealmName + "3" }; //The nicks the client will try and use.
		public static int Port = 6667; //The port the client will connect to.

		public static Privilege RequiredStaffPriv = Privilege.HalfOp;
		public static string UserName = "Mokbot";

		public static bool PerformOnConnect = true;
		public static string[] PerformMethods = { "!msg #asda Just testing Perform" };

		public static bool CallOnChannelJoin = true;

		public static string[] CallOnJoin = {
                                                "#asda:!someCommand;!AnotherCommand arg1 arg2;#WCellCommand arg1 arg2",
                                                "#chan2:!someCommand;!AnotherCommand arg1 arg2;#WCellCommand arg1 arg2"
                                            };
	}
}