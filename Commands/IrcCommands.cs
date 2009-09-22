using System;
using System.Collections;
using Squishy.Irc;
using WCell.RealmServer;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Stats;
using WCell.RealmServer.Global;
using Squishy.Irc.Commands;
using WCellAddon.IRCAddon.Voting;

namespace WCellAddon.IRCAddon.Commands
{
    #region Custom IrcCommands

    #region StatsCommand
    public class StatsCommand : Command
    {
        public static bool MuteChannelWhenPostingStats = false;

        public StatsCommand()
            : base("RealmStats")
        {
            Usage = "RealmStats";
            Description = "Prints the stats of the realm to the channel";
        }

        public override void Process(CmdTrigger trigger)
        {
            if (MuteChannelWhenPostingStats && trigger.Channel != null)
            {
                trigger.Irc.CommandHandler.Mode(trigger.Channel, "+Nm", trigger.Channel.Name);
            }
            RealmStats.Init();
            foreach (var line in RealmStats.Instance.GetFullStats())
            {
                trigger.Target.Msg(line);
            }

            if (MuteChannelWhenPostingStats && trigger.Channel != null)
            {
                trigger.Irc.CommandHandler.Mode(trigger.Channel, "-Nm", trigger.Channel.Name);
            }
        }
    }

    #endregion

    #region UpdateStatusCommand
    public class UpdateStatusCommand : Command
    {
        public UpdateStatusCommand()
            : base("UpdateStatus")
        {
            Usage = "UpdateStatus";
            Description = "Command to update the status when it wasn't done automatically";
        }

        public override void Process(CmdTrigger trigger)
        {
            var chan = trigger.Channel;
            if (trigger.Args.HasNext)
            {
                chan = trigger.Irc.GetChannel(trigger.Args.NextWord());
            }

            if (chan != null)
            {
                // Bypassing boolean AutomaticTopicUpdating (manual update should work always)
                if (IrcConnection.AutomaticTopicUpdating == false)
                {
                    IrcConnection.AutomaticTopicUpdating = true;
                    IrcConnection.UpdateTopic(trigger.Channel);
                    IrcConnection.AutomaticTopicUpdating = false;
                }
                else
                {
                    IrcConnection.UpdateTopic(trigger.Channel);
                }
            }
        }
    }

    #endregion

    #region RealmInfoCommand

    public class RealmInfoCommand : Command
    {
        public RealmInfoCommand()
            : base("ServerStats")
        {
            Usage = "ServerStats";
            Description = "Command to show simple server stats";
        }

        public override void Process(CmdTrigger trigger)
        {
            trigger.Reply("Server has been running for {0}.", RealmServer.RunTime);
            trigger.Reply("There are {0} players online (Alliance: {1}, Horde: {2}).",
                          World.CharacterCount, World.AllianceCharCount, World.HordeCharCount);
        }
    }

    #endregion

    #region RealmRatesCommand

    public class RealmRatesCommand : Command
    {
        public RealmRatesCommand()
            : base("Rates")
        {
            Usage = "Rates";
            Description = "Displays the realm rates";
        }

        public override void Process(CmdTrigger trigger)
        {
            trigger.Target.Msg("The XP factor for killing NPCs: " + XpGenerator.DefaultXpLevelFactor);
            trigger.Target.Msg("The XP factor for exploration: " + XpGenerator.ExplorationXpFactor);
        }
    }

    #endregion

    #region ClearQueueCommand

    public class ClearQueueCommand : Command
    {
        public ClearQueueCommand()
            : base("ClearQueue", "CQ")
        {
            Usage = "ClearSendQueue";
            Description = "Command to clear the send queue. Useful if you want the bot to stop spamming";
        }

        public override void Process(CmdTrigger trigger)
        {
            var lines = trigger.Irc.Client.SendQueue.Length;
            trigger.Irc.Client.SendQueue.Clear();
            trigger.Reply("Cleared SendQueue of {0} lines", lines);
        }
    }

    #endregion

    #region VoteCommands

    public class VoteStartCommand : Command
    {
        public static char VoteAnswerPrefix = '@';
        public VoteStartCommand()
            : base("StartVote", "SV")
        {
            Usage = "StartVote [Duration (seconds)]";
            Description = "Command to start a vote. If no duration is given, will last indefinitely";
        }

        public override void Process(CmdTrigger trigger)
        {
            var chan = trigger.Channel;
            var copyStream = trigger.Args.CloneStream();
            var duration = trigger.Args.NextInt();
            string question;
            if(duration > 0)
                question = trigger.Args.Remainder;
            else
            {
                question = copyStream.Remainder;
            }

            if (chan != null)
            {
                if (VoteMgr.Votes.ContainsKey(chan))
                {
                    trigger.Reply("There is already an active vote on this channel");
                    return;
                }

                trigger.Reply("Starting vote: {0}", question);
                trigger.Reply("You can vote using {0}yes or {0}no", VoteAnswerPrefix);
                var voteMgr = VoteMgr.Mgr;

                // If we have a duration given, use it.
                if (duration > 0)
                {
                    voteMgr.StartNewVote(chan, question, duration);
                }
                else { voteMgr.StartNewVote(chan, question); }

            }
            else
            {
                trigger.Reply("You can only open a vote on a valid channel (the command must come from the channel).");
            }
        }
    }

    public class EndVoteCommand : Command
    {
        public EndVoteCommand()
            : base("EndVote", "EV")
        {
            Usage = "EndVote [vote]";
            Description = "Command to end a single vote or all votes. If no vote is defined, will dispose of all open votes.";
        }

        public override void Process(CmdTrigger trigger)
        {
            var chan = trigger.Channel;
            var voteChan = trigger.Args.NextWord();
            Vote vote;

            // If we have a channel given in the argument list
            if (voteChan.Length > 0)
                vote = Vote.GetVote(trigger.Irc.GetChannel(voteChan));
            vote = Vote.GetVote(chan);

            if (vote != null)
            {
                
                trigger.Reply("The vote has ended!");
                trigger.Reply(VoteMgr.Mgr.Stats(vote));
                trigger.Reply(VoteMgr.Mgr.Result(vote));
                VoteMgr.Mgr.EndVote(vote);
                return;

            }


            else
            {
                Vote[] votesArray = new Vote[VoteMgr.Votes.Values.Count];
                VoteMgr.Votes.Values.CopyTo(votesArray, 0);
                foreach (var vte in votesArray)
                    VoteMgr.Mgr.EndVote(vte);
                trigger.Reply("Disposed of {0} vote[s].", votesArray.Length);
            }
        }
    }


    public class VoteInfoCommand : Command
    {
        public VoteInfoCommand()
            : base("VoteInfo", "VI")
        {
            Usage = "VoteInfo";
            Description = "Command to show info about the current channel's vote";
        }

        public override void Process(CmdTrigger trigger)
        {
            var chan = trigger.Channel;
            var vote = Vote.GetVote(chan);
            if (vote != null)
            {
                trigger.Reply("Current vote: \"{0}\"", vote.VoteQuestion);
                trigger.Reply("There are a total of '{0}' votes, '{1}' positive, '{2}' negative.", vote.TotalVotes, vote.PositiveCount, vote.NegativeCount);
                trigger.Reply("The vote has ran for {0}.", vote.RunTimeString);
                return;
            }

            trigger.Reply("This channel has no open votes.");
        }
    }
    #endregion

    #region ToggleExceptionNotification

    public class ToggleExcNotificationCommand : Command
    {
        public ToggleExcNotificationCommand()
            : base("ToggleExcNot", "TogExc", "TEN")
        {
            Usage = "TEN [1/0]";
            Description = "Command to toggle whether the user accepts exception notifications or not";
        }

        public override void Process(CmdTrigger trigger)
        {
            var uArgs = trigger.User.Args as WCellArgs;
            var ans = trigger.Args.NextInt();
            if(uArgs != null)
            {
                if(ans == -1)
                {
                    if (uArgs.AcceptExceptionEchos)
                    {
                        uArgs.AcceptExceptionEchos = false;
                        return;
                    }
                    uArgs.AcceptExceptionEchos = true;
                    return;
                }
                if(ans == 0)
                {
                    uArgs.AcceptExceptionEchos = false;
                    return;
                }
                if(ans > 0)
                {
                    uArgs.AcceptExceptionEchos = true;
                }
            }
        }
    }

    #endregion

    #region ResyncAuth

    public class ResyncAuthCommand : Command
    {
        public ResyncAuthCommand()
            : base("ResyncAuth", "RA")
        {
            Usage = "ResyncAuth [-a (all]";
            Description = "Command to resync your (or everybody's authentication information)";
        }

        public override void Process(CmdTrigger trigger)
        {
            var usr = trigger.User;
            var mod = trigger.Args.NextWord();
            var authMgr = trigger.Irc.AuthMgr;

            if(trigger.Text.Trim().Length == 0 && usr.IsAuthenticated)
            {
                usr.Args = null;
                authMgr.ResolveAuth(usr);
            }

            if(mod != null)
            {
                foreach(var user in trigger.Irc.Users)
                {
                    if(user.Value.IsAuthenticated)
                    {
                        user.Value.Args = null;
                        authMgr.ResolveAuth(user.Value);
                    }
                }
            }
        }
    }

    #endregion
    #endregion
}