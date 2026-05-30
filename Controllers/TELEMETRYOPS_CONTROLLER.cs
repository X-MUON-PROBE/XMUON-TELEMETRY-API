using Microsoft.AspNetCore.Mvc;
using System.Data;
using TELEMETRY_API.DB_HANDLING;
using static TELEMETRY_API.DB_HANDLING.DATASTRUCTS;
using TELEMETRY_API.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace TELEMETRY_API.Controllers
{
    [ApiController]
    [Route("telemetry")]
    public class TELEMETRYOPS_CONTROLLER : Controller
    {
        DB_HANDLER DBHandlerEngine;
	    private readonly IHubContext<MissionsDashboardWSSHub> _hubContext;

        public TELEMETRYOPS_CONTROLLER(IHubContext<MissionsDashboardWSSHub> hubContext)
        {
            //REQUIRES ENVIRONMENT VARIABLES SETUP (FORMAT - IPADDRESS|||PORT|||USERNAME|||PASSWORD)
            string[] DBAUTH_ENVVARIABLE_SPLIT = Environment.GetEnvironmentVariable("XMUON_TELEMETRY_DBDATA").Split("|||");
            string DBIPAddress = DBAUTH_ENVVARIABLE_SPLIT[0];
            string DBPort = DBAUTH_ENVVARIABLE_SPLIT[1];
            string DBUserName = DBAUTH_ENVVARIABLE_SPLIT[2];
            string DBPassword = DBAUTH_ENVVARIABLE_SPLIT[3];
            DBHandlerEngine = new DB_HANDLER($"Host={DBIPAddress};Port={DBPort};Username={DBUserName};Password={DBPassword};Database=XMUONPROBE-MISSIONDB");

	    _hubContext = hubContext;
        }

        [HttpGet("missionInit/{missionName}")]
        public IActionResult DBInitMission([FromRoute] string missionName)
        {
            int rowsAffected = DBHandlerEngine.PGSQLRunNonQuery($"CALL INSERT_MISSION_RECORD(" +
                $"'{missionName}'" +
		$");");

            DataTable updatedMissionList = DBHandlerEngine.PGSQLRunQuery("SELECT * FROM VW_MISSIONS;");

            int missionID;

            if (!int.TryParse(updatedMissionList.Rows[updatedMissionList.Rows.Count - 1][0].ToString(), out missionID))
            {
                return BadRequest("Failed to retrieve the new mission ID after insertion.");
            }

            return Ok($"{{ \"rowsAffected\": {rowsAffected}, \"missionID\": {missionID} }}");
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
                missionData.MISSION_ACTIVENESS_STATE = bool.Parse(row[3].ToString());

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
                logData.geigerCountsPerSecond = float.Parse(row[1].ToString());
		        logData.geigerCountsPerMinute = int.Parse(row[2].ToString());
                logData.geigerDose = float.Parse(row[3].ToString());
                logData.temperature = float.Parse(row[4].ToString());
                logData.atmPressure = float.Parse(row[5].ToString());
                logData.altitude = float.Parse(row[6].ToString());
                logData.accelVector = new struct_accelerationVector {
                    ax = float.Parse(row[7].ToString()),
                    ay = float.Parse(row[8].ToString()),
                    az = float.Parse(row[9].ToString()),
                };
                logData.gyroVector = new struct_gyroscopeVector
                {
                    gx = float.Parse(row[10].ToString()),
                    gy = float.Parse(row[11].ToString()),
                    gz = float.Parse(row[12].ToString()),
                };
                logData.magneticFieldVector = new struct_magneticFieldVector
                {
                    mx = float.Parse(row[13].ToString()),
                    my = float.Parse(row[14].ToString()),
                    mz = float.Parse(row[15].ToString()),
                };
                logData.headingFloat = float.Parse(row[16].ToString());
                logData.gyroChipTemperature = float.Parse(row[17].ToString());
		        logData.logTimestamp = DateTime.Parse(row[18].ToString());
                logData.airDensity = float.Parse(row[19].ToString());

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

            DataTable tempAndATMPressOverAltitudeDist = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION;" +
                $"CALL CALC_TEMP_ATMPRESS_OVER_ALTITUDE_DISTRIBUTION({missionID}, 'CURSOR');" +
                $"FETCH ALL FROM \"CURSOR\";" +
                $"COMMIT;", true);

            List<struct_tempAndATMPressureDist> distibution = new List<struct_tempAndATMPressureDist>();

            foreach (DataRow row in tempAndATMPressOverAltitudeDist.Rows)
            {
                struct_tempAndATMPressureDist distData = new struct_tempAndATMPressureDist();
                distData.altitude = float.Parse(row[0].ToString());
                distData.temperature = float.Parse(row[1].ToString());
                distData.atmPressure = float.Parse(row[2].ToString());
                distibution.Add(distData);
            }

            DataTable numericGeigerStats = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION;" +
                $"CALL CALC_NUMERIC_GEIGER_STATS({missionID}, 'CURSOR');" +
                $"FETCH ALL FROM \"CURSOR\";" +
                $"COMMIT;", true);

            struct_numericGeigerStats StructNumericGeigerStats = new struct_numericGeigerStats();

            StructNumericGeigerStats.TOTAL_GEIGER_COUNTS = int.Parse((numericGeigerStats.Rows[0])[0].ToString());
            StructNumericGeigerStats.AVG_ACTIVITY = float.Parse((numericGeigerStats.Rows[0])[1].ToString());
            StructNumericGeigerStats.MAX_ACTIVITY = float.Parse((numericGeigerStats.Rows[0])[2].ToString());
            StructNumericGeigerStats.MAX_GEIGER_DOSE = float.Parse((numericGeigerStats.Rows[0])[3].ToString());

            DataTable numericAtmStatsTable = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION;" +
                $"CALL CALC_NUMERIC_ATM_STATS({missionID}, 'CURSOR');" +
                $"FETCH ALL FROM \"CURSOR\";" +
                $"COMMIT;", true);

            struct_numericAtmStats StructNumericAtmStats = new struct_numericAtmStats();

            StructNumericAtmStats.MAX_ALTITUDE = float.Parse((numericAtmStatsTable.Rows[0])[0].ToString());
            StructNumericAtmStats.MAX_TEMPERATURE = float.Parse((numericAtmStatsTable.Rows[0])[1].ToString());
            StructNumericAtmStats.MIN_TEMPERATURE = float.Parse((numericAtmStatsTable.Rows[0])[2].ToString());
            StructNumericAtmStats.MAX_PRESSURE = float.Parse((numericAtmStatsTable.Rows[0])[3].ToString());
            StructNumericAtmStats.MIN_PRESSURE = float.Parse((numericAtmStatsTable.Rows[0])[4].ToString());

            if (foundMatch)
            {
                missionDataPackage = new mission_dataPackage
                {
                    missionData = targetMission,
                    missionMeasurementRecords = missionLOGS,
                    tempAndATMPressureDistribution = distibution,
                    numericGeigerStats = StructNumericGeigerStats,
                    numericAtmStats = StructNumericAtmStats
                };
            }

            return Ok(missionDataPackage);
        }

        [HttpPost("storeMeasurementRecord")]
        public async Task<IActionResult> DBStoreMeasurementRec([FromBody] _struct_arduinoMeasurementDataPacket packageJSON)
        {
            int rowsAffected = DBHandlerEngine.PGSQLRunNonQuery($"CALL LOG_TELEMETRY_RECORD(" +
                $"{packageJSON.missionID}," +
                $"{packageJSON.totalGeigerCounts}," +
                $"{packageJSON.geigerCountsPerSecond}," +
                $"{packageJSON.temperature}," +
                $"{packageJSON.atmPressure}," +
                $"{packageJSON.altitude}," +
                $"{packageJSON.accelVector.ax}," +
                $"{packageJSON.accelVector.ay}," +
                $"{packageJSON.accelVector.az}," +
                $"{packageJSON.gyroVector.gx}," +
                $"{packageJSON.gyroVector.gy}," +
                $"{packageJSON.gyroVector.gz}," +
                $"{packageJSON.magneticFieldVector.mx}," +
                $"{packageJSON.magneticFieldVector.my}," +
                $"{packageJSON.magneticFieldVector.mz}," +
                $"{packageJSON.gyroChipTemperature});");


		    DataTable LAST_LOG = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION; CALL GET_MISSION_DATA ('{packageJSON.missionID}', 'cursor'); FETCH ALL FROM \"cursor\"; COMMIT;", true);

            struct_measurementDataPacket logData = new struct_measurementDataPacket();
            logData.totalGeigerCounts = int.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][0].ToString());
            logData.geigerCountsPerSecond = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][1].ToString());
		    logData.geigerCountsPerMinute = int.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][2].ToString());
            logData.geigerDose = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][3].ToString());
            logData.temperature = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][4].ToString());
            logData.atmPressure = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][5].ToString());
            logData.altitude = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][6].ToString());
            logData.accelVector = new struct_accelerationVector {
                ax = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][7].ToString()),
                ay = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][8].ToString()),
                az = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][9].ToString()),
            };
            logData.gyroVector = new struct_gyroscopeVector
            {
                gx = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][10].ToString()),
                gy = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][11].ToString()),
                gz = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][12].ToString()),
            };
            logData.magneticFieldVector = new struct_magneticFieldVector
            {
                mx = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][13].ToString()),
                my = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][14].ToString()),
                mz = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][15].ToString()),
            };
            logData.headingFloat = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][16].ToString());
            logData.gyroChipTemperature = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][17].ToString());
		    logData.logTimestamp = DateTime.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][18].ToString());
            logData.airDensity = float.Parse(LAST_LOG.Rows[LAST_LOG.Rows.Count - 1][19].ToString());

            DataTable activeConnexions = DBHandlerEngine.PGSQLRunQuery($"BEGIN TRANSACTION; CALL GET_ACTIVE_MISSION_DASHBOARD_CONNEXIONS ({packageJSON.missionID}, 'cursor'); FETCH ALL FROM \"cursor\"; COMMIT;", true);

		    foreach (DataRow row in activeConnexions.Rows)
		    {
                await _hubContext.Clients.Client(row[1].ToString()).SendAsync("ReceiveDashboardUpdate", logData);
            }
		
            return Ok($"{{ \"rowsAffected\": {rowsAffected} }}");
        }
    }
}
