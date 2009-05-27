using System;
using System.Collections.Generic;
using Squishy.Irc;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core;
using WCell.RealmServer;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;
using WCell.RealmServer.Help.Tickets;

namespace WCellAddon.IRCAddon
{
    public class WCellUser : IUser, ITicketHandler
    {
        private readonly RealmAccount m_Acc;
        private readonly List<ChatChannel> m_channels = new List<ChatChannel>();

        public WCellUser(RealmAccount acc, IrcUser user)
        {
            IrcUser = user;
            m_Acc = acc;
            EntityId = EntityId.GetPlayerId(CharacterRecord.NextId());
        }

        public IrcUser IrcUser { get; private set; }

        #region Implementation of IEntity

        public EntityId EntityId { get; private set; }

        #endregion

        #region Implementation of INamed

        public string Name
        {
            get { return "Irc Bot"; }
        }

        #endregion

        #region Implementation of IPacketReceiver

        /// <summary>
        /// Unimportant
        /// </summary>
        /// <param name="packet"></param>
        public void Send(RealmPacketOut packet)
        {
        }

        #endregion

        #region Implementation of IGenericChatTarget

        public void SendMessage(string message)
        {
            IrcUser.Msg(message);
        }

        public void SendMessage(IChatter sender, string message)
        {
            IrcUser.Msg("[" + sender.Name + "] " + message);
        }

        #endregion

        #region Implementation of IChatter

        public ChatTag ChatTag
        {
            get { return Role.IsStaff ? ChatTag.GM : ChatTag.None; }
        }

        public ChatLanguage SpokenLanguage
        {
            get { return ChatLanguage.Universal; }
        }

        //void IChatter.ReceiveMessage(IChatter sender, ChatChannel chan, string message)
        //{

        //}
        #endregion

        #region Implementation of IHasRole

        public RoleGroup Role
        {
            get { return m_Acc.Role; }
        }

        #endregion

        #region Implementation of IUser

        public bool Ignores(IUser user)
        {
            return false;
        }

        /// <summary>
        /// We have no Target by default
        /// </summary>
        public Unit Target
        {
            get { return null; }
        }

        public BaseCommand<RealmServerCmdArgs> SelectedCommand { get; set; }

        /// <summary>
        /// List of ingame Channels this User is in, is going to be important
        /// when you want to let them join Channels.
        /// </summary>
        public List<ChatChannel> ChatChannels
        {
            get { return m_channels; }
        }

        /// <summary>
        /// Doesn't quite matter
        /// </summary>
        public FactionGroup FactionGroup
        {
            get { return FactionGroup.Horde; }
        }

        public ClientLocale Locale
        {
            get { return m_Acc.Locale; }
        }

        #endregion

        public Ticket HandlingTicket
        {
            get; set;
        }

        #region ITicketHandler Members

        // Only staff members can handle tickets anyway. 
        // Therefor they cannot call ticket handling commands without being staff
        public bool MayHandle(Ticket ticket)
        {
            return true;
        }

        #endregion
    }
}