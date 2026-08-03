-- ============================================================
-- Migration 003 — Helper ID Card fields
-- Adds MobileNo, Designation, BloodGroup, AadhaarNo columns
-- to the Helpers table.  All statements are idempotent.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Helpers' AND COLUMN_NAME = 'MobileNo')
    ALTER TABLE [dbo].[Helpers] ADD [MobileNo] NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Helpers' AND COLUMN_NAME = 'Designation')
    ALTER TABLE [dbo].[Helpers] ADD [Designation] NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Helpers' AND COLUMN_NAME = 'BloodGroup')
    ALTER TABLE [dbo].[Helpers] ADD [BloodGroup] NVARCHAR(10) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Helpers' AND COLUMN_NAME = 'AadhaarNo')
    ALTER TABLE [dbo].[Helpers] ADD [AadhaarNo] NVARCHAR(20) NULL;
GO
