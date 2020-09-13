using VersionControlSnapshot.Adapters;
using Framework.Data;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VersionControlSnapshot.Managers
{

    public class DatabaseManager
    {
        private const string ORACLE_DATA_ACCESS_PROVIDER_NAME = "Oracle.DataAccess.Client";
        public const string CONNECTION_NAME_KEY = "SNAPCON";
        private static IDatabase _dataBase;
        private string _dbOwner;

        public DatabaseManager()
        {
            this._dbOwner = Properties.Settings.Default.DatabaseOwner;
            _dataBase = null;
        }

        public IDatabase GetDataBase()
        {
            return GetDataBase(CONNECTION_NAME_KEY);
        }

        public IDatabase GetDataBase(string connectionName)
        {

            if (_dataBase != null)
                return _dataBase;

            ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionSettings != null)
            {
                if (connectionSettings.ProviderName == ORACLE_DATA_ACCESS_PROVIDER_NAME)
                {
                    _dataBase = new OracleDataAccessAdapter(connectionSettings.ConnectionString);
                }
            }
            if (_dataBase == null)
                _dataBase = DatabaseFactory.GetDatabase();

            return _dataBase;
        }

        public void FinalizeConnection()
        {
            if (_dataBase is OracleDataAccessAdapter)
            {
                ((OracleDataAccessAdapter)_dataBase).CloseConnection();
            }
        }
    }
}
