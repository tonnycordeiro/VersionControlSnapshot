using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VersionControlSnapshot.Models
{
    public class NetworkPathAccess
    {
        private string _netAddress;
        private string _netUser;
        private string _netPassword;

        public NetworkPathAccess(string netAddress, string netUser, string netPassword)
        {
            _netAddress = netAddress;
            _netUser = netUser;
            _netPassword = netPassword;
        }

        public void LogoutNetworkPath()
        {
            Process process;
            ProcessFactory processFactory = new ProcessFactory();

            process = processFactory.getCommandLineBackgroundProcessForLogout(this._netAddress);
            process.Start();
            process.WaitForExit();
        }

        public void LoginNetworkPath()
        {
            Process process;
            ProcessFactory processFactory = new ProcessFactory();

            process = processFactory.getCommandLineBackgroundProcessForLogin(
                this._netAddress,
                this._netUser,
                this._netPassword);

            process.Start();
            process.WaitForExit();
        }

    }
}
