-- Migration: Add GPS Check-In columns to Attendance table
-- Run this script once against your database before deploying the updated application.

ALTER TABLE [dbo].[Attendance]
    ADD [Latitude]    FLOAT          NULL,
        [Longitude]   FLOAT          NULL,
        [GpsAccuracy] FLOAT          NULL,
        [CheckInTime] DATETIME       NULL,
        [Address]     NVARCHAR(MAX)  NULL,
        [Status]      NVARCHAR(50)   NULL;
GO
