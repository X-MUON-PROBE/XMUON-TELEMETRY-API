using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
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
            //REQUIRES ENVIRONMENT VARIABLES SETUP (FORMAT - IPADDRESS|||PORT|||USERNAME|||PASSWORD)
            string[] DBAUTH_ENVVARIABLE_SPLIT = Environment.GetEnvironmentVariable("XMUON_TELEMETRY_DBDATA").Split("|||");
            string DBIPAddress = DBAUTH_ENVVARIABLE_SPLIT[0];
            string DBPort = DBAUTH_ENVVARIABLE_SPLIT[1];
            string DBUserName = DBAUTH_ENVVARIABLE_SPLIT[2];
            string DBPassword = DBAUTH_ENVVARIABLE_SPLIT[3];
            DBHandlerEngine = new DB_HANDLER($"Host={DBIPAddress};Port={DBPort};Username={DBUserName};Password={DBPassword};Database=XMUONPROBE-MISSIONDB");
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

        [HttpGet("getMissionList/")]
        public IActionResult DBGetMissionList()
        {
            DataTable results = DBHandlerEngine.PGSQLRunQuery("SELECT * FROM VW_MISSIONS;");
            List<struct_mission> missionList = new List<struct_mission>();

            foreach (DataRow row in results.Rows)
            {
                struct_mission missionData = new struct_mission();
                missionData.MISSION_ID = int.Parse(row[0].ToString());
                missionData.MISSION_NAME = row[1].ToString();
                missionData.MISSION_START_TIMESTAMP = DateTime.Parse(row[2].ToString());

                missionList.Add(missionData);
            }

            return Ok(missionList);
        }

        [HttpGet("getMissionData_{missionID}")]
        public IActionResult DBGetMissionData([FromRoute] int missionID)
        {
            DataTable results = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION;" +
                $"CALL GET_MISSION_DATA({missionID}, 'CURSOR');" +
                $"FETCH ALL FROM \"CURSOR\";" +
                $"COMMIT;", true);

            mission_dataPackage missionDataPackage = new mission_dataPackage { };
            List<struct_measurementDataPacket> missionLOGS = new List<struct_measurementDataPacket>();

            foreach (DataRow row in results.Rows)
            {
                struct_measurementDataPacket logData = new struct_measurementDataPacket();
                logData.totalGeigerCounts = int.Parse(row[0].ToString());
                logData.geigerCountsPerMinute = int.Parse(row[1].ToString());
                logData.geigerDose = float.Parse(row[2].ToString());
                logData.temperature = float.Parse(row[3].ToString());
                logData.atmPressure = float.Parse(row[4].ToString());
                logData.altitude = float.Parse(row[5].ToString());
                logData.accelVector = new struct_accelerationVector {
                    ax = float.Parse(row[6].ToString()),
                    ay = float.Parse(row[7].ToString()),
                    az = float.Parse(row[8].ToString()),
                };
                logData.gyroVector = new struct_gyroscopeVector
                {
                    gx = float.Parse(row[9].ToString()),
                    gy = float.Parse(row[10].ToString()),
                    gz = float.Parse(row[11].ToString()),
                };
                logData.magneticFieldVector = new struct_magneticFieldVector
                {
                    mx = float.Parse(row[12].ToString()),
                    my = float.Parse(row[13].ToString()),
                    mz = float.Parse(row[14].ToString()),
                };
                logData.headingFloat = float.Parse(row[15].ToString());
                logData.gyroChipTemperature = float.Parse(row[16].ToString());

                missionLOGS.Add(logData);
            }

            DataTable results2 = DBHandlerEngine.PGSQLRunQuery("SELECT * FROM VW_MISSIONS;");
            List<struct_mission> missionList = new List<struct_mission>();

            foreach (DataRow row in results2.Rows)
            {
                struct_mission missionData = new struct_mission();
                missionData.MISSION_ID = int.Parse(row[0].ToString());
                missionData.MISSION_NAME = row[1].ToString();
                missionData.MISSION_START_TIMESTAMP = DateTime.Parse(row[2].ToString());

                missionList.Add(missionData);
            }

            struct_mission targetMission = new struct_mission { };
            bool foundMatch = false;

            for(int i = 0; i < missionList.Count; i++)
            {
                if (missionList[i].MISSION_ID == missionID)
                {
                    targetMission = missionList[i];
                    foundMatch = true;
                    break;
                }
            }

            if (foundMatch)
            {
                missionDataPackage = new mission_dataPackage
                {
                    missionData = targetMission,
                    missionMeasurementRecords = missionLOGS
                };
            }

            return Ok(missionDataPackage);
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