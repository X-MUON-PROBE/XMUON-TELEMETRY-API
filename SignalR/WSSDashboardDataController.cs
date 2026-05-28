using Microsoft.AspNetCore.SignalR;
using TELEMETRY_API.DB_HANDLING;
using static TELEMETRY_API.DB_HANDLING.DATASTRUCTS;

namespace TELEMETRY_API.SignalR
{
    public class MissionsDashboardWSSHub : Hub
    {
        DB_HANDLER DBHandlerEngine;

        public MissionsDashboardWSSHub()
        {
            string[] DBAUTH_ENVVARIABLE_SPLIT = Environment.GetEnvironmentVariable("XMUON_TELEMETRY_DBDATA").Split("|||");
            string DBIPAddress = DBAUTH_ENVVARIABLE_SPLIT[0];
            string DBPort = DBAUTH_ENVVARIABLE_SPLIT[1];
            string DBUserName = DBAUTH_ENVVARIABLE_SPLIT[2];
            string DBPassword = DBAUTH_ENVVARIABLE_SPLIT[3];
            DBHandlerEngine = new DB_HANDLER($"Host={DBIPAddress};Port={DBPort};Username={DBUserName};Password={DBPassword};Database=XMUONPROBE-MISSIONDB");
        }

        public async Task SendMessage(struct_measurementDataPacket newMeasurementPacket, string TargetConnectionId) { }

        public override async Task OnConnectedAsync()
        {
            HttpContext requestURLContext = Context.GetHttpContext();

            if(requestURLContext != null)
            {
                int missionID;
                string connexionID = Context.ConnectionId;

                if (!int.TryParse(requestURLContext.Request.Query["missionID"], out missionID))
                {
                    Console.WriteLine("Invalid missionID provided in connection query string.");
                    Context.Abort();
                    return;
                }

                DBHandlerEngine.PGSQLRunNonQuery($"CALL CREATE_NEW_DASHBOARD_CONNEXION ({missionID}, '{connexionID}');");
            }
            else Console.WriteLine("Failed to retrieve HttpContext for the connection.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connexionID = Context.ConnectionId;
            DBHandlerEngine.PGSQLRunNonQuery($"CALL DELETE_DASHBOARD_CONNEXION ('{connexionID}');");

            await base.OnDisconnectedAsync(exception);
        }
    }
}