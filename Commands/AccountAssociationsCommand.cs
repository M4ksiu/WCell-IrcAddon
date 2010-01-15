using WCell.Constants.Updates;
using WCell.RealmServer.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.Constants;
using WCell.Util.Commands;

namespace IRCAddon.Commands
{
    #region Staff on IRC managing commands

    public class AccountAssociationCommand : RealmServerCommand
    {
        protected AccountAssociationCommand()
        {
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override bool NeedsCharacter
        {
            get { return false; }
        }

        protected override void Initialize()
        {
            Init("ManageStaff", "MG");
            EnglishDescription = "Manages the stafflist on IRC";
        }

        #region Nested type: AddAccountAssociationCommand

        public class AddAccountAssociationCommand : SubCommand
        {

            protected AddAccountAssociationCommand()
            {
            }

            protected override void Initialize()
            {
                Init("Add", "A");
                EnglishDescription = "Command to add an account association to the list";
                ParamInfo = "<IrcAuthName> <AccountName>";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var ircAuthName = trigger.Text.NextWord();
                var accName = trigger.Text.NextWord();

                if (!AccountAssociationsList.AccAssDict.ContainsKey(ircAuthName))
                {
                    AccountAssociationsList.AccAssDict.Add(ircAuthName, accName);
                    AccountAssociationsList.SaveDictionary("AccAssList.xml", AccountAssociationsList.AccAssDict);
                    // Making sure it actually exists now
                    if (AccountAssociationsList.AccAssDict.ContainsKey(ircAuthName))
                    {
                        trigger.Reply("Added a new account association. IrcAuthName: " + ircAuthName + " AccountName: " + accName);
                    }
                }
                else
                {
                    trigger.Reply("An association with the given key '" + ircAuthName + "' already exists. \nCheck command failed exception for                                                                                                               more info");
                }
            }
        }

        #endregion

        #region Nested type: ListAccountAssociationsCommand

        public class ListAccountAssociationsCommand : SubCommand
        {

            protected override void Initialize()
            {
                Init("List", "L");
                EnglishDescription = "Command to list all current account associations in the file";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (AccountAssociationsList.AccAssDict != null)
                {
                    foreach (var association in AccountAssociationsList.AccAssDict)
                    {
                        trigger.Reply("IrcAuthName: " + association.Key + " AccountName: " + association.Value);
                    }
                    trigger.Reply("Total number of associations in the file: " + AccountAssociationsList.AccAssDict.Count);
                }
                else
                {
                    trigger.Reply("There are no associations in the file");
                }
            }
        }

        #endregion

        #region Nested type: RemoveAccountAssociationCommand

        public class RemoveAccountAssociationCommand : SubCommand
        {

            protected RemoveAccountAssociationCommand()
            {
            }

            protected override void Initialize()
            {
                Init("Remove", "R");
                EnglishDescription = "Command to remove the account association with the given IrcAuthName";
                ParamInfo = "<IrcAuthName>";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var ircAuthName = trigger.Text.NextWord();
                var accName = trigger.Text.NextWord();

                if (AccountAssociationsList.AccAssDict.ContainsKey(ircAuthName))
                {
                    trigger.Reply("Removed " + ircAuthName + " with matching AccountName: " + accName);
                    AccountAssociationsList.AccAssDict.Remove(ircAuthName);
                    AccountAssociationsList.SaveDictionary("AccAssList.xml", AccountAssociationsList.AccAssDict);
                }
                else
                {
                    trigger.Reply("An association with the given key '" + ircAuthName + "' does not exist");
                }
            }
        }

        #endregion
    }

    #endregion

    #region AddTempAccountAssociationCommand

    public class AddTempAccountAssociationCommand : RealmServerCommand
    {

        protected AddTempAccountAssociationCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Player; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        protected override void Initialize()
        {
            Init("AddAccount", "AddAcc");
            EnglishDescription = "Adds the character to the account associations list of Irc for";
            ParamInfo = "<IrcAuthName> <InGameAccountName";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var ircAuthName = trigger.Text.NextWord();
            var accName = trigger.Text.NextWord();

            if (accName == trigger.Args.User.Name)
            {
                if (!AccountAssociationsList.AccAssDict.ContainsKey(ircAuthName))
                {
                    AccountAssociationsList.AccAssDict.Add(ircAuthName, accName);
                    // Making sure it actually exists now
                    if (AccountAssociationsList.AccAssDict.ContainsKey(ircAuthName))
                    {
                        trigger.Reply("Added a new account association. IrcAuthName: " + ircAuthName + " AccountName: " + accName);
                    }
                }
                else
                {
                    trigger.Reply("An association with the given key '" + ircAuthName + "' already exists.");
                }
            }

            else
            {
                trigger.Reply("Param <AccountName> does not match your accountname. \nYou can only add an association matching to your account");
            }
        }
    }

    #endregion
}