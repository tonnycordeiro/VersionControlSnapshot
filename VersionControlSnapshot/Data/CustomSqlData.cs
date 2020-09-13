using VersionControlSnapshot.Managers;
using Framework.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;

namespace VersionControlSnapshot.Data
{
    public class CustomSqlData
    {
        private string _connectionName;

        public CustomSqlData(string connectionName)
        {
            _connectionName = connectionName;
        }

        public string ConvertSqlResultToXML(string sql)
        {
            DatabaseManager databaseManager = new DatabaseManager();
            string xmlResult = String.Empty;
            try
            {

                IDatabase _dataBase = databaseManager.GetDataBase(this._connectionName);

                using (TransactionScope scope = new TransactionScope())
                {
                    _dataBase.CreateSqlStringCommand(sql); //databaseManager.InputDBOwnerInSQL(
                    using (IDataReader dr = _dataBase.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(dr);
                        StringWriter xmlWriter = new StringWriter();
                        dataTable.WriteXml(xmlWriter);

                        xmlResult = xmlWriter.ToString();
                    }
                }

            }
            catch
            {
                throw;
            }


            databaseManager.FinalizeConnection();
            return xmlResult;
        }
    }
}
