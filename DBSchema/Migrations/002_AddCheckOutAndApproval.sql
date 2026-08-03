-- ============================================================
-- Migration 002 — GPS Check-Out columns + Manager Approval
-- Run this script ONCE against your database before deploying.
-- All statements are idempotent (safe to re-run).
-- ============================================================

-- ── 1. Convert legacy Time column from TIME(7) to FLOAT ───────────────────
--    The application model uses double? for Time (decimal hours, e.g. 7.5).
--    TIME(7) stores wall-clock times, not durations — convert to FLOAT.
--    If Time already contains wall-clock data, those values will be lost;
--    back up the column first if needed.
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Attendance'
      AND COLUMN_NAME = 'Time'
      AND DATA_TYPE   = 'time'
)
BEGIN
    -- Clear any existing TIME values before type conversion to avoid conversion errors
    UPDATE [dbo].[Attendance] SET [Time] = NULL WHERE [Time] IS NOT NULL;
    ALTER TABLE [dbo].[Attendance] ALTER COLUMN [Time] FLOAT NULL;
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
