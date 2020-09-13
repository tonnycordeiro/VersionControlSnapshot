using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Builders
{
    public class MapFolderFileBuilder : FileBuilder
    {
        private string _sourceFullPath;

        public MapFolderFileBuilder(string rootPath, string sourceFullPath, string targetFolderPath) : base(rootPath, targetFolderPath)
        {
            this._sourceFullPath = sourceFullPath;
        }

        public override void BuildFiles()
        {
            try {

                string targetFullPath = ExplorerFilesUtil.GetFullPath(RootPath,this.TargetFolderPath);
                ExplorerFilesUtil.GrantFullAccess(targetFullPath);
                ExplorerFilesUtil.Delete(targetFullPath);
                ExplorerFilesUtil.Copy(this._sourceFullPath, targetFullPath);

            }
            catch
            {
                throw;
            }
        }
    }
}
