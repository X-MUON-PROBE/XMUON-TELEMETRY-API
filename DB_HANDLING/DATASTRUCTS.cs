using System;

namespace TELEMETRY_API.DB_HANDLING
{
    public class DATASTRUCTS
    {
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

        public struct struct_measurementDataPacket
        {
            public int totalGeigerCounts { get; set; }
            public int geigerCountsPerMinute { get; set; }
            public float geigerDose { get; set; }
            public float temperature { get; set; }
            public float atmPressure { get; set; }
            public float altitude { get; set; }
            public struct_accelerationVector accelVector { get; set; }
            public struct_gyroscopeVector gyroVector { get; set; }
            public float gyroChipTemperature { get; set; }
        }
    }
}