using Npgsql;

namespace TELEMETRY_API.DB_HANDLING
{
    public class DB_HANDLER
    {
        string connString = "";

        public DB_HANDLER(string _connString)
        {
            connString = _connString;
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