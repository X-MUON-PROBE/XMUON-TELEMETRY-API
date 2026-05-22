using Npgsql;
using System.Data;

namespace TELEMETRY_API.DB_HANDLING
{
    public class DB_HANDLER
    {
        string connString = "";

        public DB_HANDLER(string _connString)
        {
            connString = _connString;
        }

        public DataTable PGSQLRunQuery(string commandSQL, bool hasCursor = false)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();

            NpgsqlCommand QueryCommand = new NpgsqlCommand(commandSQL, conn);
            NpgsqlDataReader DataReader = QueryCommand.ExecuteReader();

            if(hasCursor) DataReader.NextResult();

            DataTable DataResults = new DataTable();
            DataResults.Load(DataReader);

            conn.Close();

            return DataResults;
        }

        public int PGSQLRunNonQuery(string commandSQL)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();

            NpgsqlCommand nonQueryCommand = new NpgsqlCommand(commandSQL, conn);
            int rowsAffected = nonQueryCommand.ExecuteNonQuery();

            conn.Close();

            return rowsAffected;
        }
    }
}