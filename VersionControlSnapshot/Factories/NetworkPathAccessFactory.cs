using VersionControlSnapshot.Managers;
using VersionControlSnapshot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VersionControlSnapshot.Factories
{
    public class NetworkPathAccessFactory
    {
        public const string PARENT_TAG_NAME = "NetworkPathAccessSettings";
        public const string NETWORK_PATH_ACCESS_ITEM_TAG = "NetworkPathAccessItem";
        public const string NET_ADDRESS_ATTRIBUTE_TAG = "NetAddress";
        public const string NET_USER_ATTRIBUTE_TAG = "NetUser";
        public const string NET_PASSWORD_ATTRIBUTE_TAG = "NetPassword";
        private XmlDocument _xmlConfig;

        public NetworkPathAccessFactory(string cfgFullPath)
        {
            this._xmlConfig = new XmlDocument();
            if (cfgFullPath != null)
                this._xmlConfig.Load(cfgFullPath);
        }

        public List<NetworkPathAccess> GetNetworkPathAccessList()
        {
            List<NetworkPathAccess> networkPathAccessList = new List<NetworkPathAccess>();

            XmlNodeList elemList = this._xmlConfig.GetElementsByTagName(NETWORK_PATH_ACCESS_ITEM_TAG);
            foreach (XmlNode xNode in elemList)
            {
                NetworkPathAccess mapFolderFileBuilder =
                    new NetworkPathAccess(xNode[NET_ADDRESS_ATTRIBUTE_TAG].InnerText,
                                                 xNode[NET_USER_ATTRIBUTE_TAG].InnerText,
                                                 xNode[NET_PASSWORD_ATTRIBUTE_TAG].InnerText
                                  );

                networkPathAccessList.Add(mapFolderFileBuilder);
            }

            return networkPathAccessList;
        }
    }
}
