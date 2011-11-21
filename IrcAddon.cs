using System;
using System.Linq;
using WCell.Core.Addons;
using System.Globalization;
using WCell.Core.Initialization;
using WCell.RealmServer;

namespace IRCAddon
{
    public class IrcAddon : WCellAddonBase
    {
    	private static IrcAddon _irc;
    	public IrcConnection[] Connections = new IrcConnection[5];

    	public static IrcAddon Instance
    	{
			get { return _irc; }
    	}

		public IrcAddon()
		{
			_irc = this;
		    RealmServer.Shutdown += TearDown;
		}

    	public override string Name
        {
            get { return "IRC implementing Addon"; }
        }

        public override string Author
        {
            get { return "Villem aka Mokrago"; }
        }

        public override string Website
        {
            get { return "http://wcell.org/forum/member.php?u=839"; }
        }

        public override string ShortName
        {
            get { return "Irc"; }
        }

        public override bool UseConfig
        {
            get { return true; }
        }

        public override string GetLocalizedName(CultureInfo culture)
        {
            return "Just an addon for allowing use of WCell and its commands on Irc.";
        }

        [Initialization(InitializationPass.Last)]
        public static void PerformAutoSave()
        {
            Instance.Config.Save();
        }

        public override void TearDown()
        {
            Instance.Config.Save();

            foreach (var con in Connections.Where(con => con != null))
            {
                con.Client.DisconnectNow();
                con.TearDown();
            }
        }
    }
}