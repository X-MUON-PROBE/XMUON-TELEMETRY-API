using Microsoft.AspNetCore.SignalR;
using TELEMETRY_API.DB_HANDLING;
using static TELEMETRY_API.DB_HANDLING.DATASTRUCTS;

namespace TELEMETRY_API.SignalR
{
    public class MissionsDashboardWSSHub : Hub
    {
        public async Task SendData(struct_measurementDataPacket newMeasurementPacket, string TargetConnectionId)
        {
            await Clients.Client(TargetConnectionId).SendAsync("ReceiveDashboardUpdate", newMeasurementPacket);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
