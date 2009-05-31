using System;
using Squishy.Irc;

namespace WCellAddon.IRCAddon.Voting
{
    public interface IVote : IDisposable
    {
        /// <summary>
        /// The timespan since creation to now.
        /// </summary>
        TimeSpan RunTime { get; }

        /// <summary>
        /// The duration of the vote. If null, the vote lasts indefinitely
        /// </summary>
        int Duration { get; set; }

        /// <summary>
        /// The current question being under voting
        /// </summary>
        string VoteQuestion { get; }

        /// <summary>
        /// The channel the voting takes place in
        /// </summary>
        IrcChannel Channel { get; }

        /// <summary>
        /// The number of "yes"s for this vote
        /// </summary>
        int PositiveCount { get; set; }

        /// <summary>
        ///  The number of "no"s for this vote
        /// </summary>
        int NegativeCount { get; set; }

        /// <summary>
        /// The number of total votes
        /// </summary>
        int TotalVotes { get; }

        /// <summary>
        /// Use this method to check whether the user can vote on this
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if user hasn't voted on this vote yet</returns>
        bool CanVote(IrcUser user);
    }
}