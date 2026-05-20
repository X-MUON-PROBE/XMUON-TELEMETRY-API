using Microsoft.AspNetCore.Mvc;
using TELEMETRY_API.DB_HANDLING;
using static TELEMETRY_API.DB_HANDLING.DATASTRUCTS;

namespace TELEMETRY_API.Controllers
{
    [ApiController]
    [Route("telemetry")]
    public class TELEMETRYOPS_CONTROLLER : Controller
    {
        DB_HANDLER DBHandlerEngine;

        public TELEMETRYOPS_CONTROLLER()
        {
            string DBIPAddress = "192.168.0.200";
            string DBPort = "5432";
            string DBPassword = "$%Hyp3r-FLUX-c@P@CITOR%$";
            DBHandlerEngine = new DB_HANDLER($"Host={DBIPAddress};Port={DBPort};Username=postgres;Password={DBPassword};Database=XMUONPROBE-MISSIONDB");
        }

        [HttpGet("missionInit/{missionName}")]
        public IActionResult DBInitMission([FromRoute] string missionName)
        {
            int rowsAffected = DBHandlerEngine.PGSQLRunNonQuery($"CALL INSERT_MISSION_RECORD(" +
                $"'{missionName}'," +
                $"'{DateTime.Now.Date.Day}/{DateTime.Now.Date.Month}/{DateTime.Now.Date.Year} {DateTime.Now.TimeOfDay.Hours}:{DateTime.Now.TimeOfDay.Minutes}:{DateTime.Now.TimeOfDay.Seconds}'" +
                $");");

            return Ok($"{{ \"rowsAffected\": {rowsAffected} }}");
        }

        [HttpPost("storeMeasurementRecord")]
        public IActionResult DBStoreMeasurementRec([FromBody] struct_measurementDataPacket packageJSON)
        {
            int rowsAffected = DBHandlerEngine.PGSQLRunNonQuery($"CALL LOG_TELEMETRY_RECORD(" +
                $"1," +
                $"{packageJSON.totalGeigerCounts}," +
                $"{packageJSON.geigerCountsPerMinute}," +
                $"{packageJSON.temperature}," +
                $"{packageJSON.atmPressure}," +
                $"{packageJSON.altitude}," +
                $"{packageJSON.accelVector.ax}," +
                $"{packageJSON.accelVector.ay}," +
                $"{packageJSON.accelVector.az}," +
                $"{packageJSON.gyroVector.gx}," +
                $"{packageJSON.gyroVector.gy}," +
                $"{packageJSON.gyroVector.gz}," +
                $"1," +
                $"1," +
                $"1," +
                $"{packageJSON.gyroChipTemperature});");

            return Ok($"{{ \"rowsAffected\": {rowsAffected} }}");
        }
    }
}