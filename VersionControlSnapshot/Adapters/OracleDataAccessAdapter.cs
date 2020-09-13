using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Framework.Data;
using Oracle.DataAccess.Client;

namespace VersionControlSnapshot.Adapters
{
    public class OracleDataAccessAdapter : IDatabase
    {
        private OracleConnection _connection;
        private string _query;

        public OracleDataAccessAdapter(string connectionString)
        {
            _connection = new OracleConnection(connectionString);
        }

        public void CloseConnection()
        {
            this._connection.Close();
        }

        public void AddInParameter(string name, DbType dbType)
        {
            throw new NotImplementedException();
        }

        public void AddInParameter(string name, DbType dbType, object value)
        {
            throw new NotImplementedException();
        }

        public void AddInParameter(string name, DbType dbType, string sourceColumn, DataRowVersion sourceVersion)
        {
            throw new NotImplementedException();
        }

        public void AddOutParameter(string name, DbType dbType, int size)
        {
            throw new NotImplementedException();
        }

        public void AddParameter(string name, DbType dbType, ParameterDirection direction, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            throw new NotImplementedException();
        }

        public void AddParameter(string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            throw new NotImplementedException();
        }

        public void AddReturnParameter(string name, DbType dbType, int size)
        {
            throw new NotImplementedException();
        }

        public void AssignParameters(object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public string BuildParameterName(string name)
        {
            throw new NotImplementedException();
        }

        public void CreateSqlStringCommand(string query)
        {
            this._query = query;
        }

        public void CreateStoredProcCommand(string storedProcedureName)
        {
            throw new NotImplementedException();
        }

        public void CreateStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public void CreateStoredProcCommandWithSourceColumns(string storedProcedureName, params string[] sourceColumns)
        {
            throw new NotImplementedException();
        }

        public void DiscoverParameters()
        {
            throw new NotImplementedException();
        }

        public DataSet ExecuteDataSet()
        {
            throw new NotImplementedException();
        }

        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            throw new NotImplementedException();
        }

        public DataSet ExecuteDataSet(string storedProcedureName, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader()
        {
            this._connection.Open();
            OracleCommand command = new OracleCommand(this._query, this._connection);
            return command.ExecuteReader();
        }

        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string storedProcedureName, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public DbDataAdapter GetDataAdapter()
        {
            throw new NotImplementedException();
        }

        public object GetParameterValue(string name)
        {
            throw new NotImplementedException();
        }

        public void LoadDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            throw new NotImplementedException();
        }

        public void LoadDataSet(DataSet dataSet, string tableName)
        {
            throw new NotImplementedException();
        }

        public void LoadDataSet(DataSet dataSet, string[] tableNames)
        {
            throw new NotImplementedException();
        }

        public void LoadDataSet(string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public void SetParameterValue(string name, object value)
        {
            throw new NotImplementedException();
        }
    }
}
