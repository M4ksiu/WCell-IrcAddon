using System;
using WCell.Core.Addons;
using System.Globalization;

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

        public override void TearDown()
        {
            foreach(var con in Connections)
            {
            	con.Client.DisconnectNow();
            	con.TearDown();
            }
        }
    }
}