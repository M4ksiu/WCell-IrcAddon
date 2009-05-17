using System;
using WCell.RealmServer.Commands;
using WCell.RealmServer;
using Squishy.Irc;

namespace WCell.IRCAddon
{
    /// <summary>
    /// Add objects of this class to any authed user.
    /// If user.Args is null, it means the user may not use Commands (probably not authed etc).
    /// </summary>
    public class WCellArgs : IIrcUserArgs
    {
        /// <summary>
        /// Use this ctor on authed users only
        /// </summary>
        /// <param name="account"></param>
        public WCellArgs(RealmAccount account)
        {
            if (account == null)
            {
                throw new NullReferenceException("account");
            }
            Account = account;
        }

        public RealmAccount Account { get; set; }

        public RealmServerCmdArgs CmdArgs { get; set; }
    }
}