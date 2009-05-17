using Squishy.Irc;
using WCell.Core;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Stats;
using WCell.RealmServer.Global;
using Squishy.Irc.Commands;

namespace WCell.IRCAddon.Commands
{
     #region Custom IrcCommands

    #region StatsCommand
    public class StatsCommand : Command
    {
        public static bool MuteChannelWhenPostingStats = false;

        public StatsCommand() : base("RealmStats")
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
        public UpdateStatusCommand() : base("UpdateStatus")
        {
            Usage = "UpdateStatus";
            Description = "Command to update the status when it wasn't done automatically";
        }

        public override void Process(CmdTrigger trigger)
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

    #endregion

    #region RealmInfoCommand

    public class RealmInfoCommand : Command
    {
        public RealmInfoCommand() : base("ServerStats")
        {
            Usage = "ServerStats";
            Description = "Command to show simple server stats";
        }

        public override void Process(CmdTrigger trigger)
        {
            trigger.Reply("Server has been running for {0}.", RealmServer.RealmServer.RunTime);
            trigger.Reply("There are {0} players online (Alliance: {1}, Horde: {2}).",
                          World.CharacterCount, World.AllianceCharCount, World.HordeCharCount);
        }
    }

    #endregion

    #region RealmRatesCommand

    public class RealmRatesCommand : Command
    {
        public RealmRatesCommand() : base("Rates")
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
            trigger.Irc.Client.SendQueue.Clear();
            trigger.Reply("Cleared SendQueue");
        }
    }

    #endregion

    #endregion
}