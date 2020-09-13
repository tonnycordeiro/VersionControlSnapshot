using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Builders
{
    public class ProcessCommandLineFileBuilder : FileBuilder
    {
        string _processFileName;
        string _processArguments;
        private ProcessStartInfo _processStartInfo;
        private Process _process;
        private ProcessFactory _processFactory;

        public ProcessCommandLineFileBuilder(string rootPath, string targetFolderPath, string processFileName, string processArguments) : base(rootPath, targetFolderPath)
        {
            this._process = null;
            this._processFileName = processFileName;
            this._processArguments = processArguments;
            this._processStartInfo = new ProcessStartInfo(this._processFileName, this._processArguments);
            this._processFactory = new ProcessFactory();
        }

        public Process Process { get { return _process; } private set { this._process = value; } }

        public string ProcessFileName { get { return _processFileName; } private set { _processFileName = value; } }

        public string ProcessArguments { get { return _processArguments; } private set { _processArguments = value; } }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public override void BuildFiles()
        {
            try
            {
                this._process = this._processFactory.getCommandLineBackgroundProcess(this._processStartInfo);
                this._process.Start();
            }catch
            {
                throw;
            }
        }

    }
}
