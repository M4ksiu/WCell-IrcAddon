using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using WCell.Util;

namespace WCell.IRCAddon
{
    public class AccountAssociationsList : XmlConfig<AccountAssociationsList>
    {
        public static string AccountAssociationFileName = "AccAssList.xml";

        [XmlElement("Associations")]
        public AccountAssociation[] Associations { get; set; }

        public static string FilePath
        {
            get
            {
                if (IrcAddon.Context.Descriptor.Directory != null)
                {
                    string addonDir = IrcAddon.Context.Descriptor.Directory;
                    var path = Path.Combine(addonDir, AccountAssociationFileName);
                    return path;
                }
                return "";
            }
        }

        public static Dictionary<string, string> AccAssDict { get; set; }

        #region Load

        public static Dictionary<string, string> LoadDictionary(string filename)
        {
            var path = FilePath;

            var dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                // add Associations from file
                var cfg = Load(path);
                if (cfg != null && cfg.Associations != null)
                {
                    foreach (var association in cfg.Associations)
                    {
                        dict[association.IrcAuthName] = association.AccountName;
                    }
                }
            }
            AccAssDict = dict;
            return dict;
        }

        #endregion

        #region Save

        public static void SaveDictionary(string filename, Dictionary<string, string> associations)
        {
            //AccAssDict = associations;
            var array = associations.TransformArray(pair => new AccountAssociation(pair.Key, pair.Value));
            var cfg = new AccountAssociationsList {Associations = array};
            cfg.SaveAs(FilePath);
        }

        #endregion
    }

    public class AccountAssociation
    {
        public string AccountName;
        public string IrcAuthName;

        public AccountAssociation(string ircAuthName, string accountName)
        {
            IrcAuthName = ircAuthName;
            AccountName = accountName;
        }

        public AccountAssociation()
        {
        }
    }
}

//http://pastebin.com/m218e19f1
//http://pastebin.com/m4c2223e8