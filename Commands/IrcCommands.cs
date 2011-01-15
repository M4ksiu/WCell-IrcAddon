using System;
using System.Collections;
using IRCAddon.Voting;
using Squishy.Irc;
using WCell.RealmServer;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Stats;
using WCell.RealmServer.Global;
using Squishy.Irc.Commands;
using WCell.Util.Commands;

namespace IRCAddon.Commands
{
    #region Custom IrcCommands

    #region StatsCommand
    public class StatsCommand : IrcCommand
    {
        public static bool MuteChannelWhenPostingStats = false;

        public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            if (MuteChannelWhenPostingStats && trigger.Args.Channel != null)
            {
                trigger.Args.IrcClient.CommandHandler.Mode(trigger.Args.Channel, "+Nm", trigger.Args.Channel.Name);
            }
            RealmStats.Init();
            foreach (var line in RealmStats.Instance.GetFullStats())
            {
                trigger.Args.Target.Msg(line);
            }

            if (MuteChannelWhenPostingStats && trigger.Args.Channel != null)
            {
                trigger.Args.IrcClient.CommandHandler.Mode(trigger.Args.Channel, "-Nm", trigger.Args.Channel.Name);
            }
        }

		protected override void Initialize()
		{
			Init("RealmStats");
            EnglishDescription = "Prints the stats of the realm to the channel";
		}
	}

    #endregion

    #region UpdateStatusCommand
    public class UpdateStatusCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
    		Init("UpdateStatus");
			EnglishDescription = "Command to update the status when it wasn't done automatically";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var chan = trigger.Args.Channel;
            if (trigger.Text.HasNext)
            {
                chan = trigger.Args.IrcClient.GetChannel(trigger.Text.NextWord());
            }

            if (chan != null)
            {
                // Bypassing boolean AutomaticTopicUpdating (manual update should work always)
                if (IrcConnection.AutomaticTopicUpdating == false)
                {
                    IrcConnection.AutomaticTopicUpdating = true;
                    IrcConnection.UpdateTopic(trigger.Args.Channel);
                    IrcConnection.AutomaticTopicUpdating = false;
                }
                else
                {
                    IrcConnection.UpdateTopic(trigger.Args.Channel);
                }
            }
        }
    }

    #endregion

    #region RealmInfoCommand

	//public class RealmInfoCommand : IrcCommand
	//{
	//    public RealmInfoCommand()
	//        : base("ServerStats")
	//    {
	//        Usage = "ServerStats";
	//        Description = "Command to show simple server stats";
	//    }

	//    protected override void Initialize()
	//    {
	//        Init("ServerStats");
	//        Description = "Command to show simple server stats";
	//    }

	//    public override void Process(CmdTrigger<IrcCmdArgs> trigger)
	//    {
	//        trigger.Reply("Server has been running for {0}.", RealmServer.RunTime);
	//        trigger.Reply("There are {0} players online (Alliance: {1}, Horde: {2}).",
	//                      World.CharacterCount, World.AllianceCharCount, World.HordeCharCount);
	//    }
	//}

    #endregion

    #region RealmRatesCommand

    public class RealmRatesCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
    		Init("Rates");
			EnglishDescription = "Displays the realm rates";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            trigger.Args.Target.Msg("The XP factor for killing NPCs: " + XpGenerator.DefaultXpLevelFactor);
            trigger.Args.Target.Msg("The XP factor for exploration: " + XpGenerator.ExplorationXpFactor);
        }
    }

    #endregion

    #region ClearQueueCommand

    public class ClearQueueCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
    		Init("ClearQueue", "CQ");
			EnglishDescription = "Command to clear the send queue. Useful if you want the bot to stop spamming";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var lines = trigger.Args.IrcClient.Client.SendQueue.Length;
            trigger.Args.IrcClient.Client.SendQueue.Clear();
            trigger.Reply("Cleared SendQueue of {0} lines", lines);
        }
    }

    #endregion

    #region VoteCommands

    public class VoteStartCommand : IrcCommand
    {
        public static char VoteAnswerPrefix = '@';
    	protected override void Initialize()
    	{
    		Init("StartVote", "SV");
    		EnglishParamInfo = "[Duration (seconds)]";
			EnglishDescription = "Command to start a vote. If no duration is given, will last indefinitely";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var chan = trigger.Args.Channel;
            var copyStream = trigger.Text.CloneStream();
            var duration = trigger.Text.NextInt();
            string question;
            if(duration > 0)
                question = trigger.Text.Remainder;
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

    public class EndVoteCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
    		Init("EndVote", "EV");
			EnglishDescription = "Command to end a single vote or all votes. If no vote is defined, will dispose of all open votes.";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var chan = trigger.Args.Channel;
            var voteChan = trigger.Text.NextWord();
            Vote vote;

            // If we have a channel given in the argument list
            if (voteChan.Length > 0)
                vote = Vote.GetVote(trigger.Args.IrcClient.GetChannel(voteChan));
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


    public class VoteInfoCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
			Init("VoteInfo", "VI");
			EnglishDescription = "Command to show info about the current channel's vote";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var chan = trigger.Args.Channel;
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

    public class ToggleExcNotificationCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
			Init("ToggleExcNotification", "ToggleExcNot", "TogExc", "TEN");
			EnglishDescription = "Command to toggle whether the user accepts exception notifications or not";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var uArgs = trigger.Args.User.Args as WCellArgs;
            var ans = trigger.Text.NextInt();
            if(uArgs != null)
            {
                if(ans == -1)
                {
                    if (uArgs.AcceptExceptionEchos)
                    {
                        uArgs.AcceptExceptionEchos = false;
                        trigger.Reply("Exception echoing to user " + trigger.Args.User.Nick + ": " + uArgs.AcceptExceptionEchos);
                        return;
                    }
                    uArgs.AcceptExceptionEchos = true;
                    trigger.Reply("Exception echoing to user " + trigger.Args.User.Nick + ": " + uArgs.AcceptExceptionEchos);
                    return;
                }
                if(ans == 0)
                {
                    uArgs.AcceptExceptionEchos = false;
                    trigger.Reply("Exception echoing has been set to false.");
                    return;
                }
                if(ans > 0)
                {
                    uArgs.AcceptExceptionEchos = true;
                }
                trigger.Reply("Exception echoing to user " + trigger.Args.User.Nick + ": " + uArgs.AcceptExceptionEchos);
            }
            trigger.Reply("User is not matched to a server account.");
        }
    }

    #endregion

    #region ResyncAuth

    public class ResyncAuthCommand : IrcCommand
    {
    	protected override void Initialize()
    	{
    		Init("ResyncAuth");
			EnglishParamInfo = " [-a (all]";
            EnglishDescription = "Command to resync your (or everybody's authentication information)";
    	}

    	public override void Process(CmdTrigger<IrcCmdArgs> trigger)
        {
            var usr = trigger.Args.User;
            var mod = trigger.Text.NextWord();
            var authMgr = trigger.Args.IrcClient.AuthMgr;

            if(trigger.Text.String.Trim().Length == 0 && usr.IsAuthenticated)
            {
                usr.Args = null;
                //authMgr.ResolveAuth(usr);
            }

            if(mod != null)
            {
                foreach(var user in trigger.Args.IrcClient.Users)
                {
                    if(user.Value.IsAuthenticated)
                    {
                        user.Value.Args = null;
                        //authMgr.ResolveAuth(user.Value);
                    }
                }
            }
        }
    }

    #endregion
    #endregion
}