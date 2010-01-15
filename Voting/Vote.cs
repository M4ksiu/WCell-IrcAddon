using System.Collections.Generic;
using System.Timers;
using Squishy.Irc;
using System;

namespace IRCAddon.Voting
{
    public class Vote : IVote
    {
        private string m_Vote;
        private int m_PositiveCount = 0;
        private int m_NegativeCount = 0;
        public HashSet<IrcUser> votedUsers = new HashSet<IrcUser>();
        private bool disposed;
        private IrcChannel m_Chan;
        private int m_Duration;
        private DateTime m_CreationTime;
        private Timer m_Timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="voteQuestion">The string of the vote.</param>
        /// <param name="channel">The channel where the vote takes place.</param>
        /// <param name="lifeSpan">If null, the vote lasts indefinitely. Else sets the time the vote lasts before ending (seconds)</param>
        public Vote(string voteQuestion, IrcChannel channel, int lifeSpan)
        {
            m_Vote = voteQuestion;
            m_Chan = channel;
            m_CreationTime = DateTime.Now;

            m_Timer = new Timer(lifeSpan * 1000);
            m_Timer.AutoReset = false;
            m_Timer.Start();
            m_Timer.Elapsed += m_Timer_Elapsed;
        }

        public Vote(string voteQuestion, IrcChannel channel)
        {
            m_Vote = voteQuestion;
            m_Chan = channel;
            m_CreationTime = DateTime.Now;

        }

        void m_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Channel.Msg(VoteMgr.Mgr.Stats(this));
            Channel.Msg(VoteMgr.Mgr.Result(this));
            VoteMgr.Mgr.EndVote(this);
        }

        /// <summary>
        /// The timespan since creation to now.
        /// </summary>
        public TimeSpan RunTime
        {
            get { return DateTime.Now.Subtract(m_CreationTime); }
        }

        /// <summary>
        /// Returns the formatted string representation of the lifespan of the vote
        /// </summary>
        public string RunTimeString
        {
            get
            {
                return (RunTime.Days + " Days, " + RunTime.Hours + " Hours, " + RunTime.Minutes + " Minutes, " +
                               RunTime.Seconds + " Seconds");
            }
        }

        /// <summary>
        /// The duration of the voting period. If 0, the vote lasts indefinitely.
        /// </summary>
        public int Duration
        {
            get
            {
                if (!m_Timer.Enabled) return 0;
                else
                {
                    return (int)m_Timer.Interval;
                }
            }
            set
            {
                if (value != 0)
                    m_Timer.Interval = value;
                m_Timer.Enabled = false;
            }
        }
        /// <summary>
        /// The current question being under voting.
        /// </summary>
        public string VoteQuestion
        {
            get { return m_Vote; }
        }

        /// <summary>
        /// The channel the voting takes place in.
        /// </summary>
        public IrcChannel Channel
        {
            get { return m_Chan; }
        }
        /// <summary>
        /// The number of "yes"s for this vote.
        /// </summary>
        public int PositiveCount
        {
            get { return m_PositiveCount; }
            set { m_PositiveCount = value; }
        }

        /// <summary>
        ///  The number of "no"s for this vote.
        /// </summary>
        public int NegativeCount
        {
            get { return m_NegativeCount; }
            set { m_NegativeCount = value; }
        }

        /// <summary>
        /// The number of total votes.
        /// </summary>
        public int TotalVotes
        {
            get { return m_PositiveCount + m_NegativeCount; }
        }

        /// <summary>
        /// Use this method to check whether the user can vote on this.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if user hasn't voted on this vote yet</returns>
        public bool CanVote(IrcUser user)
        {
            if (votedUsers.Contains(user))
                return false;
            return true;
        }

        /// <summary>
        /// Returns the Vote from all currently open votes.
        /// </summary>
        /// <param name="channel">The channel the vote takes place in.</param>
        /// <returns></returns>
        public static Vote GetVote(IrcChannel channel)
        {
            Vote vote;
            VoteMgr.Votes.TryGetValue(channel, out vote);
            return vote;
        }

        /// <summary>
        /// Returns the vote question
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return VoteQuestion;
        }
        /// <summary>
        /// Call this after voting has finished and you don't need the object any longer.
        /// </summary>
        public void Dispose()
        {
            var vote = GetVote(Channel);
            Destroy(vote);
        }

        /// <summary>
        /// Cleans up after the vote so that it could be disposed.
        /// </summary>
        /// <param name="vote"></param>
        private void Destroy(Vote vote)
        {
            if(vote.m_Timer != null)
                vote.m_Timer.Stop();
            VoteMgr.Votes.Remove(vote.Channel);

            vote.disposed = true;
        }

    }
}
