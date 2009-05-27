using Squishy.Irc;
using WCell.RealmServer;

namespace WCellAddon.IRCAddon
{
    public class ServerStatus
    {
        #region Global Variables

        public static IrcColorCode LockedColor = IrcColorCode.Orange;
        public static IrcColorCode OfflineColor = IrcColorCode.Red;
        public static IrcColorCode OnlineColor = IrcColorCode.Green;

        #endregion

        private static string statusName;

        public static string StatusName
        {
            get { return GetStatusName(); }
        }

        public static string GetStatusName()
        {
            if (RealmServer.Instance.IsRunning && RealmServer.Instance.IsRunning
                && !RealmServer.IsShuttingDown && RealmServer.Instance.AuthClient.IsConnected)
            {
                statusName = "Online".Colorize(OnlineColor);
            }
            else
            {
                statusName = "Offline".Colorize(OfflineColor);
            }

            return statusName;
        }
    }
}