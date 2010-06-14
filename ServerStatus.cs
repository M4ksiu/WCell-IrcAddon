using Squishy.Irc;
using WCell.Constants.Login;
using WCell.RealmServer;

namespace IRCAddon
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
            if (RealmServer.Instance.IsRunning && !RealmServer.IsShuttingDown && RealmServer.Instance.AuthClient.IsConnected && RealmServerConfiguration.Status != RealmStatus.Locked)
            {
                statusName = "Online".Colorize(OnlineColor);
            }
            if (RealmServer.IsPreparingShutdown || RealmServer.IsShuttingDown)
            {
                statusName = "Offline".Colorize(OfflineColor);
            }
            if (RealmServerConfiguration.Status == RealmStatus.Locked && !RealmServer.IsShuttingDown)
            {
                statusName = "Locked".Colorize(LockedColor);
            }
            return statusName;
        }
    }
}