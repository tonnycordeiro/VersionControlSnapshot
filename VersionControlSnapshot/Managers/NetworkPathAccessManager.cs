using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VersionControlSnapshot.Managers
{
    public class NetworkPathAccessManager
    {
        private NetworkPathAccessFactory networkPathAccessFactory;
        private List<NetworkPathAccess> networkPathAccessList;

        public NetworkPathAccessManager(string networkPathAccessSettings_FilePath)
        {
            this.networkPathAccessFactory = new NetworkPathAccessFactory(networkPathAccessSettings_FilePath);
            this.networkPathAccessList = networkPathAccessFactory.GetNetworkPathAccessList();
        }

        public void ProvideNetworkAccess()
        {
            try
            {
                foreach (NetworkPathAccess networkPathAccess in networkPathAccessList)
                {
                    networkPathAccess.LoginNetworkPath();
                }
            }
            catch
            {
                throw;
            }
        }

    }
}
