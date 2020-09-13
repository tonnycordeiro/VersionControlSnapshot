using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Builders
{
    public abstract class FileBuilder
    {
        string _rootPath;
        string _targetFolderPath;

        public FileBuilder(string rootPath, string targetFolderPath, bool cleanDirectory = true)
        {
            this._rootPath = rootPath;
            this._targetFolderPath = targetFolderPath;

            /*if (cleanDirectory)
                ExplorerFilesUtil.CleanDirectory(TargetFullPath);
            else*/
            ExplorerFilesUtil.CreateDirectoryIfNotExists(TargetFullPath);
        }

        public string RootPath { get { return _rootPath; } set { this._rootPath = value; } }

        public string TargetFolderPath { get { return _targetFolderPath; } set { this._targetFolderPath = value; } }

        public string TargetFullPath
        {
            get { return ExplorerFilesUtil.GetFullPath(this.RootPath, this.TargetFolderPath); }
        }

        public abstract void BuildFiles();

    }
}
