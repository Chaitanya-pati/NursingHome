-- ============================================================
-- Migration 002 — GPS Check-Out columns + Manager Approval
-- Run this script ONCE against your database before deploying.
-- All statements are idempotent (safe to re-run).
-- ============================================================

-- ── 1. Convert legacy Time column from TIME(7) to FLOAT ───────────────────
--    The application model uses double? for Time (decimal hours, e.g. 7.5).
--    Existing TIME(7) values are preserved as decimal hours (e.g. 08:30:00
--    becomes 8.5) via a staging column — no data is lost.
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Attendance'
      AND COLUMN_NAME = 'Time'
      AND DATA_TYPE   = 'time'
)
BEGIN
    -- Step 1: add a FLOAT staging column
    ALTER TABLE [dbo].[Attendance] ADD [_TimeFloat] FLOAT NULL;
    -- Step 2: copy TIME values as decimal hours (HH + MM/60 + SS/3600)
    UPDATE [dbo].[Attendance]
    SET [_TimeFloat] =
          CAST(DATEPART(HOUR,   [Time]) AS FLOAT)
        + CAST(DATEPART(MINUTE, [Time]) AS FLOAT) / 60.0
        + CAST(DATEPART(SECOND, [Time]) AS FLOAT) / 3600.0
    WHERE [Time] IS NOT NULL;
    -- Step 3: drop the original TIME column
    ALTER TABLE [dbo].[Attendance] DROP COLUMN [Time];
    -- Step 4: rename the staging column to [Time]
    EXEC sp_rename 'dbo.Attendance._TimeFloat', 'Time', 'COLUMN';
END
GO

-- ── 2. Add GPS Check-Out columns (idempotent) ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'CheckOutTime')
    ALTER TABLE [dbo].[Attendance] ADD [CheckOutTime] DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'CheckOutLatitude')
    ALTER TABLE [dbo].[Attendance] ADD [CheckOutLatitude] FLOAT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'CheckOutLongitude')
    ALTER TABLE [dbo].[Attendance] ADD [CheckOutLongitude] FLOAT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'CheckOutGpsAccuracy')
    ALTER TABLE [dbo].[Attendance] ADD [CheckOutGpsAccuracy] FLOAT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'CheckOutAddress')
    ALTER TABLE [dbo].[Attendance] ADD [CheckOutAddress] NVARCHAR(MAX) NULL;
GO

-- ── 3. Add Manager Approval columns (idempotent) ──────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TotalHours')
    ALTER TABLE [dbo].[Attendance] ADD [TotalHours] FLOAT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'ManagerRemarks')
    ALTER TABLE [dbo].[Attendance] ADD [ManagerRemarks] NVARCHAR(MAX) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'ApprovedBy')
    ALTER TABLE [dbo].[Attendance] ADD [ApprovedBy] NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'ApprovalTimestamp')
    ALTER TABLE [dbo].[Attendance] ADD [ApprovalTimestamp] DATETIME NULL;
GO
