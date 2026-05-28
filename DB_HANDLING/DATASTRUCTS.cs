using System;

namespace TELEMETRY_API.DB_HANDLING
{
    public class DATASTRUCTS
    {
        public struct struct_mission
        {
            public int MISSION_ID { get; set; }
            public string MISSION_NAME { get; set; }
            public DateTime MISSION_START_TIMESTAMP { get; set; }
            public bool MISSION_ACTIVENESS_STATE { get; set; }
        }

        public struct struct_tempAndATMPressureDist
        {
            public float temperature { get; set; }
            public float atmPressure { get; set; }
            public float altitude { get; set; }
        }

        public struct struct_numericGeigerStats
        {
            public int TOTAL_GEIGER_COUNTS { get; set; }
            public float AVG_ACTIVITY { get; set; }
            public float MAX_ACTIVITY { get; set; }
            public float MAX_GEIGER_DOSE { get; set; }
        }

        public struct struct_numericAtmStats
        {
            public float MAX_ALTITUDE { get; set; }
            public float MAX_TEMPERATURE { get; set; }
            public float MIN_TEMPERATURE { get; set; }
            public float MAX_PRESSURE { get; set; }
            public float MIN_PRESSURE { get; set; }
        }

        public struct struct_accelerationVector
        {
            public float ax { get; set; }
            public float ay { get; set; }
            public float az { get; set; }
        }

        public struct struct_gyroscopeVector
        {
            public float gx { get; set; }
            public float gy { get; set; }
            public float gz { get; set; }
        }

        public struct struct_magneticFieldVector
        {
            public float mx { get; set; }
            public float my { get; set; }
            public float mz { get; set; }
        }

        public struct struct_measurementDataPacket
        {
            public int totalGeigerCounts { get; set; }
            public float geigerCountsPerSecond { get; set; }
	        public int geigerCountsPerMinute { get; set; }
            public float geigerDose { get; set; }
            public float temperature { get; set; }
            public float atmPressure { get; set; }
            public float airDensity { get; set; }
            public float altitude { get; set; }
            public struct_accelerationVector accelVector { get; set; }
            public struct_gyroscopeVector gyroVector { get; set; }
            public struct_magneticFieldVector magneticFieldVector { get; set; }
            public float headingFloat { get; set; }
            public float gyroChipTemperature { get; set; }
	        public DateTime logTimestamp { get; set; }
        }

        public struct _struct_arduinoMeasurementDataPacket
        {
	        public int missionID { get; set; }
            public int totalGeigerCounts { get; set; }
            public int geigerCountsPerSecond { get; set; }
            public float geigerDose { get; set; }
            public float temperature { get; set; }
            public float atmPressure { get; set; }
            public float altitude { get; set; }
            public struct_accelerationVector accelVector { get; set; }
            public struct_gyroscopeVector gyroVector { get; set; }
            public struct_magneticFieldVector magneticFieldVector { get; set; }
            public float gyroChipTemperature { get; set; }
        }

        public struct mission_dataPackage
        {
            public struct_mission missionData { get; set; }
            public List<struct_measurementDataPacket> missionMeasurementRecords { get; set; }
            public List<struct_tempAndATMPressureDist> tempAndATMPressureDistribution { get; set; }
            public struct_numericGeigerStats numericGeigerStats { get; set; }
            public struct_numericAtmStats numericAtmStats { get; set; }
        }
    }
}