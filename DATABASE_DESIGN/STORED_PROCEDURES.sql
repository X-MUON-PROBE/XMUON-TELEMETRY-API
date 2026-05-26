CREATE OR REPLACE PROCEDURE INSERT_MISSION_RECORD(
    _MISSION_NAME VARCHAR(50),
    _MISSION_START_TIMESTAMP TIMESTAMPTZ)
LANGUAGE plpgsql
AS $$
    DECLARE
    BEGIN
        INSERT INTO TELEMETRY_MISSIONS (
             MISSION_NAME,
             MISSION_START_TIMESTAMP
        )
        VALUES (
          _MISSION_NAME,
          _MISSION_START_TIMESTAMP
         );
    END
$$;

CREATE OR REPLACE PROCEDURE LOG_TELEMETRY_RECORD(
    _RECORD_MISSION_ID INT,
    _TOTAL_GEIGER_COUNTS INT,
    _GEIGER_COUNTS_PER_MINUTE INT,
    _TEMPERATURE FLOAT,
    _ATM_PRESSURE FLOAT,
    _ALTITUDE FLOAT,
    _ACCELERATION_X FLOAT,
    _ACCELERATION_Y FLOAT,
    _ACCELERATION_Z FLOAT,
    _GYRO_X FLOAT,
    _GYRO_Y FLOAT,
    _GYRO_Z FLOAT,
    _MAGNETIC_FIELD_X FLOAT,
    _MAGNETIC_FIELD_Y FLOAT,
    _MAGNETIC_FIELD_Z FLOAT,
    _GYRO_CHIP_TEMPERATURE FLOAT)
LANGUAGE plpgsql
AS $$
    DECLARE _GEIGER_DOSE FLOAT; _GEIGER_ACTIVITY FLOAT; _HEADING_RAD FLOAT; _HEADING_DEG FLOAT;
    BEGIN
        _GEIGER_ACTIVITY :=  _GEIGER_COUNTS_PER_MINUTE / 60.0;
        _GEIGER_DOSE := _GEIGER_COUNTS_PER_MINUTE * 0.00812;
        _HEADING_RAD := ATAN2(_MAGNETIC_FIELD_Y, _MAGNETIC_FIELD_X);
        _HEADING_DEG := (_HEADING_RAD * 180) / PI();

        INSERT INTO TELEMETRY_RECORDS
        (
         RECORD_MISSION_ID,
         TOTAL_GEIGER_COUNTS,
         GEIGER_COUNTS_PER_MINUTE,
         GEIGER_COUNTS_PER_SECOND,
         GEIGER_DOSE,
         TEMPERATURE,
         ATM_PRESSURE,
         ALTITUDE,
         ACCELERATION_X,
         ACCELERATION_Y,
         ACCELERATION_Z,
         GYRO_X,
         GYRO_Y,
         GYRO_Z,
         MAGNETIC_FIELD_X,
         MAGNETIC_FIELD_Y,
         MAGNETIC_FIELD_Z,
         HEADING_DEG,
         GYRO_CHIP_TEMPERATURE
        )
        VALUES
        (
         _RECORD_MISSION_ID,
         _TOTAL_GEIGER_COUNTS,
         _GEIGER_COUNTS_PER_MINUTE,
         _GEIGER_ACTIVITY,
         _GEIGER_DOSE,
         _TEMPERATURE,
         _ATM_PRESSURE,
         _ALTITUDE,
         _ACCELERATION_X,
         _ACCELERATION_Y,
         _ACCELERATION_Z,
         _GYRO_X,
         _GYRO_Y,
         _GYRO_Z,
         _MAGNETIC_FIELD_X,
         _MAGNETIC_FIELD_Y,
         _MAGNETIC_FIELD_Z,
         _HEADING_DEG,
         _GYRO_CHIP_TEMPERATURE
        );
    END
$$;

CREATE OR REPLACE PROCEDURE GET_MISSION_DATA(
    MISSION_ID INT,
    INOUT result_set REFCURSOR  -- Add an INOUT parameter for the cursor
)
LANGUAGE plpgsql
AS $$
    DECLARE
    BEGIN
        OPEN result_set FOR
        SELECT total_geiger_counts,
               GEIGER_COUNTS_PER_SECOND,
               GEIGER_COUNTS_PER_MINUTE,
               geiger_dose,
               temperature,
               atm_pressure,
               altitude,
               acceleration_x,
               acceleration_y,
               acceleration_z,
               gyro_x,
               gyro_y,
               gyro_z,
               magnetic_field_x,
               magnetic_field_y,
               magnetic_field_z,
               heading_deg,
               gyro_chip_temperature,
               RECORD_TIMESTAMP
        FROM TELEMETRY_RECORDS
        WHERE RECORD_MISSION_ID = MISSION_ID;
    END;
$$;

CREATE OR REPLACE PROCEDURE GET_DATABASE_SIZE(STORAGE_UNIT VARCHAR(2))
LANGUAGE plpgsql
AS $$
    BEGIN
        CASE
            WHEN STORAGE_UNIT = 'KB' THEN
                RAISE NOTICE '% KB', pg_database_size('XMUONPROBE-MISSIONDB') / 1024;
            WHEN STORAGE_UNIT = 'MB' THEN
                RAISE NOTICE '% MB', pg_database_size('XMUONPROBE-MISSIONDB') / power(1024, 2);
            when STORAGE_UNIT = 'GB' THEN
                RAISE NOTICE '% GB', pg_database_size('XMUONPROBE-MISSIONDB') / power(1024, 3);
            ELSE RAISE WARNING 'BAD STORAGE_UNIT ARGUMENT. STORAGE_UNIT SUPPORTS THE FOLLOWING TAGS: "KB", "MB" AND "GB"';
        END CASE;
    END;
$$;