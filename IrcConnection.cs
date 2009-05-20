/* This addon was written by Mokrago using Domii's graceously provided irc lib and was often helped out by Domii.
 * So basically I wrote 45% of it, Domii did 50% of it, and the rest 5% was pure magic (hope those magic fixes are stable :P)
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using Squishy.Irc;
using Squishy.Irc.Commands;
using Squishy.Irc.Protocol;
using Squishy.Network;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.Util.NLog;


namespace WCell.IRCAddon
{

    public class IrcConnection : IrcClient
    {
        #region Fields

        #region Public Static Fields (used for configuration)

        public static bool AnnounceAuthToUser = true;
        public static bool AutoAuth = true;
        public static bool AutomaticTopicUpdating = true;
        public static bool HideChatting = false;
        public static bool HideIncomingIrcPackets = false;
        public static bool HideOutgoingIrcPackets = false;
        public static bool ReConnectOnDisconnect = true;
        public static int ReConnectWaitTimer = 5;
        public static bool ReJoinOnKick = true;
        public static string WCellCmdPrefix = "#";
        public static bool AuthAllUsersOnJoin = false;
        public static bool UpdateTopicOnFlagAdded = true;
        public static IrcCmdCallingRange IrcCmdCallingRange = IrcCmdCallingRange.LocalChannel;
        public static int ExceptionNotificationRank = 1000;
        public static bool ExceptionNotify = true;
        //public static char IrcCmdPrefix = CommandHandler.RemoteCommandPrefix;
        public static int SendQueue
        {
            get
            {
                return ThrottledSendQueue.CharsPerSecond;
            }
            set
            {
                ThrottledSendQueue.CharsPerSecond = value;
            }
        }

        #endregion

        #region Private Fields

        //Every channel the bot has joined
        private HashSet<string> watchedChannels = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private Timer m_maintainConnTimer;

        #endregion

        #endregion
        
        public IrcConnection()
        {
            RealmServer.RealmServer.Instance.AuthClient.Disconnected += AuthClientDisconnected;
            RealmServer.RealmServer.Instance.AuthClient.Connected += AuthClientConnected;
            ProtocolHandler.PacketReceived += OnReceive;
            RealmServer.RealmServer.Shutdown += OnShutdown;
            RealmServer.RealmServer.Instance.StatusChanged += OnStatusNameChange;
            m_maintainConnTimer = new Timer(maintainCallback);
            LogUtil.ExceptionRaised += LogUtil_ExceptionRaised;
        }



        /// <summary>
        /// The Main method
        /// </summary>
        [Initialization(InitializationPass.Last, "Initializing IrcAddon")]
        public static void InitIrc()
        {
            if (!File.Exists(AccountAssociationsList.FilePath))
            {
                var list = new AccountAssociationsList();
                list.SaveAs(AccountAssociationsList.FilePath);
            }

            AccountAssociationsList.LoadDictionary(AccountAssociationsList.AccountAssociationFileName);

            Connect();
        }

        /// <summary>
        /// Method to connect to the irc network
        /// </summary>
        public static void Connect()
        {
            try
            {
                var client = new IrcConnection
                {
                    Nicks = IrcAddonConfig.Nicks,
                    UserName = IrcAddonConfig.UserName,
                    // the name that will appear in the hostmask before @ e.g. Mokbot@wcell.org
                    Info = IrcAddonConfig.Info // The info line: Mokbot@wcell.org : asd (<- this bit)
                };

                client.BeginConnect(IrcAddonConfig.Network, IrcAddonConfig.Port);
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        // Fires when the Client is fully logged on the network and the End of Motd is sent (raw 376).
        protected override void Perform()
        {
            foreach (var channel in IrcAddonConfig.ChannelList)
            {
                watchedChannels.Add(channel);
                foreach (var chan in watchedChannels)
                {
                    CommandHandler.Join(chan);
                }
            }

            IrcCommandHandler.Initialize();
            WCellUtil.Init(this);

            m_maintainConnTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void OnShutdown()
        {
            if (Client.IsConnected)
            {
                IrcChannel chan = GetChannel(IrcAddonConfig.ChannelList[0]);
                UpdateTopic(chan, chan.Topic);
            }
            Client.Disconnect();
        }

        private void OnStatusNameChange(RealmStatus status)
        {
            IrcChannel chan = GetChannel(IrcAddonConfig.ChannelList[0]);
            UpdateTopic(chan, chan.Topic);
        }

        private void AuthClientDisconnected(object sender, EventArgs e)
        {
            IrcChannel chan = GetChannel(IrcAddonConfig.ChannelList[0]);
            UpdateTopic(chan);
        }

        private void AuthClientConnected(object sender, EventArgs e)
        {
            IrcChannel chan = GetChannel(IrcAddonConfig.ChannelList[0]);
            UpdateTopic(chan);
        }

        #region OnConnecting/OnDisconnected/Packets/OnExceptionRaised

        protected override void OnConnecting()
        {
            Console.WriteLine("Connecting to {0}:{1} ...", Client.RemoteAddress, Client.RemotePort);
        }

        protected override void OnConnected()
        {
            Console.WriteLine("Connected to {0}:{1} ...", Client.RemoteAddress, Client.RemotePort);
        }

        protected void OnReceive(IrcPacket packet)
        {
            if (HideIncomingIrcPackets != true)
            {
                Console.WriteLine("<-- " + packet);
            }
        }

        protected override void OnBeforeSend(string text)
        {
            if (HideOutgoingIrcPackets != true)
            {
                Console.WriteLine("--> " + text);
            }
        }

        protected override void OnConnectFail(Exception ex)
        {
            Console.WriteLine("Connection failed: " + ex);
            Console.WriteLine("Trying to reconnect in {0}", ReConnectWaitTimer);
            StartReConnectTimer();
        }

        /// <summary>
        /// Starts the reconnect timer
        /// </summary>
        private void StartReConnectTimer()
        {
            Client.Disconnect();
            m_maintainConnTimer.Change(0, ReConnectWaitTimer * 1000);
        }

        protected override void OnDisconnected(bool conLost)
        {
            Console.WriteLine("Disconnected" + (conLost ? " (Connection lost)" : ""));
            if (conLost && ReConnectOnDisconnect)
            {
                StartReConnectTimer();
            }
        }

        protected override void OnExceptionRaised(Exception e)
        {
            Console.WriteLine(e);
        }

        #endregion

        #region User related events/Methods

        //Rejoin the channel the bot was kicked from, only called when the kicked user's name
        //matches the bot's name
        protected override void OnKick(IrcUser from, IrcChannel chan, IrcUser target, string reason)
        {
            if (target == Me && ReJoinOnKick)
            {
                try
                {
                    CommandHandler.Join(chan.Name);
                    CommandHandler.Msg(from, "Don't kick me!", Me.Nick);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        protected override void OnUserLeftChannel(IrcChannel chan, IrcUser user, string reason)
        {
            base.OnUserLeftChannel(chan, user, reason);
            Console.WriteLine("**{0} quit({1})", user.Nick, reason);
        }

        protected override void OnJoin(IrcUser user, IrcChannel chan)
        {
            //If the bot joins a channel, it will add that chan to watchedChannels
            if (user == Me)
            {
                watchedChannels.Add(chan.Name);
            }

            //Try and auth the joined user
            try
            {
                if (AnnounceAuthToUser)
                {
                    user.Msg("Resolving User...".Colorize(IrcColorCode.Red));
                }
                AuthMgr.ResolveAuth(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnUsersAdded(IrcChannel chan, IrcUser[] users)
        {
            Console.WriteLine("Topic was set {0} by {1}", chan.TopicSetTime, chan.TopicSetter);
            Console.WriteLine("Topic is: {0}", chan.Topic);

            //Only set the topic of the first channel
            if (chan.Name == IrcAddonConfig.ChannelList[0])
            {
                UpdateTopic(chan, chan.Topic);
            }

            if (AutoAuth)
            {
                if (AuthAllUsersOnJoin)
                    foreach (var usr in users)
                    {
                        if (AnnounceAuthToUser)
                        {
                            usr.Msg("Resolving User...".Colorize(IrcColorCode.Red));
                        }
                        AuthMgr.ResolveAuth(usr);
                    }
            }
        }

        // Try and update the topic of the channel after the bot has been privileged
        // Useful when the bot joined and wasn't privileged to change the topic and if
        // afterwards a user/network service/bot opped the bot, it will try and update the topic
        // Bear in mind this will update the status of *any* channel he is given op or higher priv
        protected override void OnFlagAdded(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
        {
            if(target == Me && priv >= IrcAddonConfig.RequiredStaffPriv && UpdateTopicOnFlagAdded)
            {
                Console.WriteLine("Updating topic...");
                UpdateTopic(chan);
            }
        }


        #endregion

        #region Topic/text

        protected override void OnTopic(IrcUser user, IrcChannel chan, string text, bool initial)
        {
            if (initial)
            {
                Console.WriteLine("The topic for channel {0} is {1}", chan.Name, chan.Topic);
            }
            else
            {
                Console.WriteLine("{0} changed topic in channel {1} to: {2}", user.Nick, chan.Name, text);
                UpdateTopic(chan, text);
            }
        }

        protected override void OnText(IrcUser user, IrcChannel chan, StringStream text)
        {
            var uArgs = user.Args as WCellArgs;
            if (HideChatting != true)
            {
                Console.WriteLine("<{0}> {1}", user, text);
            }
            if (user.IsAuthenticated && text.String.StartsWith(WCellCmdPrefix) && uArgs != null)
            {
                WCellUtil.HandleCommand((WCellUser)uArgs.CmdArgs.User, user, chan, text.String.TrimStart(WCellCmdPrefix.ToCharArray()));
            }

        }

        #endregion

        #region Command handling

        /// <summary>
        /// Return wether or not the given trigger may be processed.
        /// Default: Only allows if local or no user triggered it.
        /// </summary>
        public override bool MayTriggerCommand(CmdTrigger trigger, Command cmd)
        {
            var uArgs = trigger.User.Args as WCellArgs;

            if (base.MayTriggerCommand(trigger, cmd))
            {
                return true;
            }

            var chan = trigger.Channel;
            if (chan != null)
            {
                if (uArgs == null)
                {
                    if (watchedChannels.Contains(chan.Name) &&
                        chan.IsUserAtLeast(trigger.User, IrcAddonConfig.RequiredStaffPriv))
                    {
                        return CheckCmdCallingRange(trigger, chan);
                    }
                }
                else if (cmd is AuthCommand || uArgs.Account.Role.IsStaff)
                {
                    return CheckCmdCallingRange(trigger, chan);
                }
            }

            foreach (var userChan in trigger.User.Channels.Values)
            {
                if (uArgs == null)
                {
                    if (watchedChannels.Contains(userChan.Name) &&
                        (userChan.IsUserAtLeast(trigger.User, IrcAddonConfig.RequiredStaffPriv)))
                        return true;

                    else if(cmd is AuthCommand)
                        return true;
                }
                else if (cmd is AuthCommand || CheckIsStaff(trigger.User))
                {
                    return CheckCmdCallingRange(trigger, userChan);
                }
            }
            return false;
        }

        public override bool TriggersCommand(IrcUser user, IrcChannel chan, StringStream input)
        {
            if (chan != null)
            {
                if (base.TriggersCommand(user, chan, input))
                {
                    if (user.IsAuthenticated && CheckIsStaff(user))
                    {
                            return true;
                    }

                    if (chan.IsUserAtLeast(user, IrcAddonConfig.RequiredStaffPriv))
                    {
                        return true;
                    }
                }
            }
            else if(input.String.ToLower().StartsWith("!auth") || CheckIsStaff(user))
            {
                return input.ConsumeNext(CommandHandler.RemoteCommandPrefix);
            }

            return false;
        }

        protected override void OnUnknownCommandUsed(CmdTrigger trigger)
        {
            trigger.Reply("Command-Alias not found: " + trigger.Alias);
            base.OnUnknownCommandUsed(trigger);
        }

        protected override void OnCommandFail(CmdTrigger trigger, Exception ex)
        {
            Command cmd = trigger.Command;
            string[] lines = ex.ToString().Split(new[] {"\r\n|\n|\r"}, StringSplitOptions.RemoveEmptyEntries);

            trigger.Reply("Exception raised: " + lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                // TODO: automatically detect lines before sending in Client-class
                trigger.Reply(lines[i]);
            }
            trigger.Reply("Unproper command use - " + cmd.Usage + ": " + cmd.Description);
        }

        #endregion

        #region Helper methods

        private void LogUtil_ExceptionRaised(string text, Exception exception)
        {
            if (ExceptionNotify)
            {
                foreach (IrcUser user in Users.Values)
                {
                    if (user.IsAuthenticated)
                    {
                        var uArgs = user.Args as WCellArgs;
                        if (uArgs != null)
                        {
                            if (uArgs.Account.Role >= ExceptionNotificationRank)
                                Send(text, exception);
                        }
                    }
                }
            }
        }

        private void maintainCallback(object state)
        {
            if(!LoggedIn)
                InitIrc();
        }

        /// <summary>
        /// A helper method to check whether or not the user is staff.
        /// Mostly used to maintain code readability.
        /// Did not use in places I really don't want anything to go wrong
        /// and also in places where I wanted the flow of code to be 
        /// centralised for readability.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool CheckIsStaff(IrcUser user)
        {
            var uArgs = user.Args as WCellArgs;
            if(uArgs != null)
            {
                if(uArgs.Account.Role.IsStaff)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Method to check IrcCmdCallingRange and return a boolean accordingly
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="chan"></param>
        /// <returns></returns>
        private bool CheckCmdCallingRange(CmdTrigger trigger, IrcChannel chan)
        {
            if(trigger.Command is AuthCommand)
                return true;

            switch (IrcCmdCallingRange)
            {
                case IrcCmdCallingRange.Everywhere:
                    return true;

                case IrcCmdCallingRange.IsPrivilegedOnTrgt:
                    if (!trigger.Text.Contains("#"))
                        return true;

                    if(trigger.Command is JoinCommand)
                        return true;

                    else
                    {
                        //Checks whether or not the triggerer has staff priv levels on the target channel
                        var user = trigger.User;
                        var chanName = trigger.Args.CloneStream().NextWord(" ");
                        var targetChan = GetChannel(chanName);

                        if (targetChan.HasUser(user.Nick))
                        {
                            if(targetChan.IsUserAtLeast(user, IrcAddonConfig.RequiredStaffPriv))
                            return true;
                        }
                    }
                    break;

                default:
                    if (!trigger.Text.Contains("#"))
                        return true;

                    else
                    {
                        if(trigger.Command is JoinCommand)
                            return true;

                        string chanName = trigger.Args.CloneStream().NextWord(" ");
                        var asd = trigger.Args;
                        if (chan.Name == chanName)
                            return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Formats the channel's topic and adds server status
        /// Only used in OnTopic
        /// </summary>
        /// <param name="chan"></param>The channel which topic is being updated
        /// <param name="text"></param>The channel's new topic
        private void UpdateTopic(IrcChannel chan, string text)
        {
            if (chan != null)
            {
                if (AutomaticTopicUpdating && chan.Topic != text)
                {
                    if (text.Contains("Server status: "))
                    {
                        text = Regex.Replace(text, @"Server status\: [^$ ]+", "Server status: " + ServerStatus.StatusName);
                        chan.Topic = text;
                    }

                    else
                        chan.Topic = text.Trim() + " | Server status: " + ServerStatus.StatusName;
                }
            }
        }

        /// <summary>
        /// A static method 
        /// </summary>
        /// <param name="chan"></param>
        public static void UpdateTopic(IrcChannel chan)
        {
            if (chan == null) return;

            if (chan.Topic.Contains("Server status: "))
            {
                chan.Topic = Regex.Replace(chan.Topic, @"Server status\: [^$ ]+", "Server status: " + ServerStatus.StatusName);
            }

            else
            {
                chan.Topic = chan.Topic.Trim() + " | Server status: " + ServerStatus.StatusName;
            }
        }
        #endregion
    }
}