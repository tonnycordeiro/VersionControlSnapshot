using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Factories
{
    public class ProcessFactory
    {
        private const string NET_PROCESS = "net";
        private const string NET_PROCESS_LOGIN_FORMAT = @"use \\{0} {1} /user:{2}";
        private const string NET_PROCESS_LOGOUT_FORMAT = @"use \\{0} /delete";
        private string _localAdministratorUserName;
        private string _localAdministratorPassword;

        public ProcessFactory()
        {
            this._localAdministratorUserName = Properties.Settings.Default.LocalAdministratorUserName;
            this._localAdministratorPassword = Properties.Settings.Default.LocalAdministratorPassword;
        }

        public Process getCommandLineBackgroundProcess(ProcessStartInfo processStartInfo)
        {
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.Verb = "runas";

            if(!String.IsNullOrEmpty(this._localAdministratorUserName) && !String.IsNullOrEmpty(this._localAdministratorPassword))
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Password = ConvertToSecureString(this._localAdministratorPassword);
                process.StartInfo.UserName = this._localAdministratorUserName;
            }

            return process;
        }

        public SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }

        public Process getCommandLineBackgroundProcessForLogin(string networkHostName, string userName, string userPassword)
        {
            string netLoginCommand = String.Format(NET_PROCESS_LOGIN_FORMAT,
                networkHostName, userPassword, userName);
            ProcessStartInfo processStartInfo = new ProcessStartInfo(
                NET_PROCESS, netLoginCommand);

            return getCommandLineBackgroundProcess(processStartInfo);
        }

        public Process getCommandLineBackgroundProcessForLogout(string networkHostName)
        {
            string netLogoutCommand = String.Format(NET_PROCESS_LOGOUT_FORMAT,
                networkHostName);
            ProcessStartInfo processStartInfo = new ProcessStartInfo(
                NET_PROCESS, netLogoutCommand);

            return getCommandLineBackgroundProcess(processStartInfo);
        }

        public Process getWindowsCredentialsProcess(String URL, String username, String password)
        {
            Process rdcProcess = new Process();

            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe");

            rdcProcess.StartInfo.Arguments = "/generic:[Add the URL]" + URL + "/user:[Add Username]" + username + " /pass:[Add Password]" + password;

            return rdcProcess;

        }
    }

}
