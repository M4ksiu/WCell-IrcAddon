using System;
using System.Collections.Generic;
using Squishy.Irc;
using Squishy.Network;
using WCell.Util.Variables;
using WCellAddon.IRCAddon.Commands;
using WCellAddon.IRCAddon.Voting;

namespace WCellAddon.IRCAddon
{
    class VoteMgr
    {
        public delegate void VoteReceivedHandler(Vote vote, IrcUser user, bool answer);

        public event VoteReceivedHandler VoteReceived;

        private string m_VoteQuestion;

        /// <summary>
        /// Contains all open votes
        /// </summary>
        [NotVariable]
        public static Dictionary<string, Vote> Votes = new Dictionary<string, Vote>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Contains channels (and the corresponding votes) with currently active votes
        /// </summary>
        [NotVariable]
        public static Dictionary<IrcChannel, Vote> ChannelVotes = new Dictionary<IrcChannel, Vote>();

        private static VoteMgr m_Instance = new VoteMgr();

        /// <summary>
        /// The instance of the VoteMgr
        /// </summary>
        public static VoteMgr Mgr
        {
            get { return m_Instance; }
        }

        /// <summary>
        /// Start a new vote in a channel
        /// </summary>
        /// <param name="channel">The channel where the voting takes place</param>
        /// <param name="question">The string we are voting over</param>
        public bool StartNewVote(IrcChannel channel, string question)
        {
            var vote = new Vote(question, channel);
            Votes.Add(question, vote);
            ChannelVotes.Add(channel, vote);
            var args = channel.Args as IVote;
            args = vote;

            channel.TextReceived += (user, text) =>
                                        {
                                            var voteCmdAlias = VoteStartCommand.VoteAnswerPrefix;
                                            if (text.String.StartsWith(voteCmdAlias.ToString()))
                                            {
                                                var answer = text.NextWord();
                                                if (answer.TrimStart(voteCmdAlias).Equals("yes", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    AddVote(user, vote, true);
                                                    return;
                                                }

                                                if (answer.TrimStart(voteCmdAlias).Equals("no", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    AddVote(user, vote, false);
                                                }
                                            }
                                        };
            return true;
        }

        public bool StartNewVote(IrcChannel channel, string question, int durationSeconds)
        {
            var vote = new Vote(question, channel, durationSeconds);
            Votes.Add(question, vote);
            ChannelVotes.Add(channel, vote);

            channel.TextReceived += (user, text) =>
            {
                var voteCmdAlias = VoteStartCommand.VoteAnswerPrefix;
                if (text.String.StartsWith(voteCmdAlias.ToString()))
                {
                    var answer = text.NextWord();
                    if (answer.TrimStart(voteCmdAlias).Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        AddVote(user, vote, true);
                        return;
                    }

                    if (answer.TrimStart(voteCmdAlias).Equals("no", StringComparison.OrdinalIgnoreCase))
                    {
                        AddVote(user, vote, false);
                    }
                }
            };
            return true;
        }

        /// <summary>
        /// Adds a "yes" or a "no" to the given vote
        /// </summary>
        /// <param name="user">The user voting</param>
        /// <param name="vote">The vote we are handling</param>
        /// <param name="answer">True = "yes", False = "no"</param>
        private void AddVote(IrcUser user, Vote vote, bool answer)
        {
            if (VoteReceived != null)
                VoteReceived(vote, user, answer);

            if (vote.CanVote(user))
            {
                // Answer is yes
                if (answer)
                    vote.PositiveCount += 1;

                // Answer is no
                else
                {
                    vote.NegativeCount += 1;
                }

                // We add a user to it's corresponding vote so that no one can vote more than once
                vote.votedUsers.Add(user);
            }
        }

        /// <summary>
        /// Returns the stats of the vote and disposes of the vote
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        public void EndVote(Vote vote)
        {
            vote.Dispose();
        }

        public string Stats(Vote vote)
        {
            return "There are a total of '" + vote.TotalVotes + "' votes, '" + vote.PositiveCount + "' positive, '" + vote.NegativeCount + "' negative. ";
        }

        public string Result(Vote vote)
        {
            if (vote.PositiveCount > vote.NegativeCount)
                return "Vote resulted in favour of \"" + vote.VoteQuestion + "\" !";
            if (vote.PositiveCount < vote.NegativeCount)
                return "The crowd is NOT in favour of \"" + vote.VoteQuestion + "\" !";

            //vote.PositiveCount == vote.NegativeCount 
            return "It's a draw!";
        }
    }
}
