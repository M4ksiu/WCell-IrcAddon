using WCell.Core.Addons;
using System.Globalization;

namespace WCell.IRCAddon
{
    public class IrcAddon : WCellAddonBase
    {
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
            get { return "IrcAddon"; }
        }

        public override bool UseConfig
        {
            get { return true; }
        }

        public override string GetLocalizedName(CultureInfo culture)
        {
            return "Just an addon";
        }
    }
}
