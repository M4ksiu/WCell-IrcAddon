using Squishy.Irc;

namespace WCell.IRCAddon
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
            if (RealmServer.RealmServer.Instance.IsRunning && RealmServer.RealmServer.Instance.IsRegisteredAtAuthServer
                && !RealmServer.RealmServer.IsShuttingDown && RealmServer.RealmServer.Instance.AuthClient.IsConnected)
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