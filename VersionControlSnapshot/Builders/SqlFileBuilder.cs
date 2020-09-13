using VersionControlSnapshot.Data;
using VersionControlSnapshot.Utils;
using Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Builders
{
    public class SqlFileBuilder : FileBuilder
    {
        private string _sql;
        private string _fileName;
        private string _connectionName;

        public SqlFileBuilder(string rootPath, string targetFolderPath, string sql, string fileName, string connectionName) : base(rootPath, targetFolderPath)
        {
            this._sql = sql;
            this._fileName = fileName;
            this._connectionName = connectionName;
        }

        public override void BuildFiles()
        {
            try
            {
                CustomSqlData customSqlData;

                customSqlData = new CustomSqlData(this._connectionName);
                ExplorerFilesUtil.CreateNewFile(ExplorerFilesUtil.GetFullPath(this.TargetFullPath, this._fileName), customSqlData.ConvertSqlResultToXML(this._sql));
            }
            catch
            {
                throw;
            }
        }
    }
}
