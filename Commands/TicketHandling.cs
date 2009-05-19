using System.Collections;
using WCell.Constants.Factions;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.Constants;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Help.Tickets;
using WCell.Constants.Tickets;
using WCell.RealmServer.Misc;


namespace WCell.IRCAddon.Commands
{
    public class AddTestTicketCommand : RealmServerCommand
    {

        protected AddTestTicketCommand() { }

        protected override void Initialize()
        {
            Init("AddTicket", "AD");
            EnglishDescription = "Add a test ticket. Used for ticket testing only";
            ParamInfo = "<character> <TicketType> <text>";
            Enabled = true;
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.None;
            }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.Staff;
            }
        }

        /// <summary>
        /// Whether the Character argument needs to be supplied by the trigger's Args
        /// </summary>
        public override bool NeedsCharacter
        {
            get
            {
                return false;
            }
        }

        public override bool RequiresContext
        {
            get
            {
                return false;
            }
        }

        public override void Process(Util.Commands.CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var name = trigger.Text.NextWord();
            var tType = trigger.Text.NextEnum<TicketType>(" ");
            //var text = trigger.Text.RemainingWords(" ").ToString();
            var text = trigger.Text.NextWord();
            var chr = trigger.Args.GetCharArgumentOrTarget(trigger, name);

            var ticket = new Ticket(chr, text, tType);
            TicketMgr.Instance.AddTicket(ticket);
            trigger.Reply("Placeholder");
        }
    }

    public class ReplyToTicketCommand : RealmServerCommand
    {

        protected ReplyToTicketCommand() { }

        protected override void Initialize()
        {
            Init("Reply", "Rep");
            EnglishDescription = "Initiate chat with the user. Must have a ticket selected";
            Enabled = true;
        }


        public override void Process(Util.Commands.CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var ticket = trigger.Args.TicketHandler.HandlingTicket;
            if (ticket != null)
            {
                Character user = ticket.Owner;

                if (ticket.Owner == null)
                {
                    trigger.Reply("The owner of this Ticket is offline.");
                }
                else
                {
                    var me = new ChannelMember(trigger.Args.User);
                    var ircChannel = new ChatChannelGroup(FactionGroup.Invalid);
                    var chan = new ChatChannel(ircChannel);

                    chan.Invite(me, user);
                    //user.SendMessage(trigger.Args.User,
                    //                 "A staff member wants to chat with you about your ticket. Please do not leave the channel.");
                }
            }
            else
            {
                trigger.Reply("You must have a ticket selected.");
            }
        }
    }
}
