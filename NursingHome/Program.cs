using Microsoft.Data.SqlClient;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

var builder = WebApplication.CreateBuilder(args);

// Prefer DB_CONNECTION_STRING environment variable (Replit secret) over appsettings.json value.
var dbConnectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("NursingHome");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUserService,UserService>(provider =>
{
    return new UserService(dbConnectionString);
});
builder.Services.AddTransient<IConfig,Config>(provider =>
{
    return new Config(dbConnectionString);
});
builder.Services.AddTransient<IOldAge,OldAge>(provider =>
{
    return new OldAge(dbConnectionString);
});
builder.Services.AddTransient<INursingHome,HomeNursing>(provider =>
{
    return new HomeNursing(dbConnectionString);
});
builder.Services.AddTransient<IHelpers,Helpers>(provider =>
{
    return new Helpers(dbConnectionString);
});
builder.Services.AddTransient<ICashMemo,CashMemo>(provider =>
{
    return new CashMemo(dbConnectionString);
});
builder.Services.AddTransient<IAttedanceService,AttedanceService>(provider =>
{
    return new AttedanceService(dbConnectionString);
});
builder.Services.AddTransient<ISalarySlipService, SalarySlipService>(provider =>
{
    return new SalarySlipService(dbConnectionString);
});
builder.Services.AddTransient<IHomeService, HomeService>(provider =>
{
    return new HomeService(dbConnectionString);
});

var app = builder.Build();

// ── Run Attendance GPS migrations at startup (idempotent — safe every restart) ──
RunAttendanceMigrations(dbConnectionString);
// ── Add Helpers ID-card columns if missing ────────────────────────────────────
RunHelpersMigrations(dbConnectionString);
// ── Add User assignment / audit columns and tables ────────────────────────────
RunUserAssignmentMigrations(dbConnectionString);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}/{id?}");

app.Run();

// ────────────────────────────────────────────────────────────────────────────────
// Adds User assignment columns to [Users] and creates [HelperUserAssignmentHistory].
// Idempotent — safe on every boot.
// ────────────────────────────────────────────────────────────────────────────────
static void RunUserAssignmentMigrations(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("[Migration] UserAssignment: Skipped — connection string is empty.");
        return;
    }
    var statements = new[]
    {
        // Users: new profile columns
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' AND COLUMN_NAME='Email') ALTER TABLE [dbo].[Users] ADD [Email] NVARCHAR(200) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' AND COLUMN_NAME='IsActive') ALTER TABLE [dbo].[Users] ADD [IsActive] BIT NOT NULL DEFAULT 1",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' AND COLUMN_NAME='LastLogin') ALTER TABLE [dbo].[Users] ADD [LastLogin] DATETIME NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' AND COLUMN_NAME='CreatedDate') ALTER TABLE [dbo].[Users] ADD [CreatedDate] DATETIME NULL",

        // HelperUserAssignmentHistory table
        @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='HelperUserAssignmentHistory')
          CREATE TABLE [dbo].[HelperUserAssignmentHistory] (
              [Id]               INT IDENTITY(1,1) PRIMARY KEY,
              [HelperId]         INT NOT NULL,
              [AssignedUserName] NVARCHAR(110) NULL,
              [Action]           NVARCHAR(50)  NULL,
              [AssignedBy]       NVARCHAR(110) NULL,
              [AssignedDate]     DATETIME      NULL,
              [RemovedBy]        NVARCHAR(110) NULL,
              [RemovedDate]      DATETIME      NULL,
              [Notes]            NVARCHAR(500) NULL
          )"
    };
    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        foreach (var sql in statements)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("[Migration] UserAssignment migration complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Migration] UserAssignment WARNING: {ex.Message}");
    }
}

// ────────────────────────────────────────────────────────────────────────────────
// Adds ID-card columns to [Helpers] if they don't already exist.
// Idempotent — safe to run on every boot.
// ────────────────────────────────────────────────────────────────────────────────
static void RunHelpersMigrations(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("[Migration] Helpers: Skipped — connection string is empty.");
        return;
    }
    var statements = new[]
    {
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Helpers' AND COLUMN_NAME='MobileNo') ALTER TABLE [dbo].[Helpers] ADD [MobileNo] NVARCHAR(50) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Helpers' AND COLUMN_NAME='Designation') ALTER TABLE [dbo].[Helpers] ADD [Designation] NVARCHAR(100) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Helpers' AND COLUMN_NAME='BloodGroup') ALTER TABLE [dbo].[Helpers] ADD [BloodGroup] NVARCHAR(20) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Helpers' AND COLUMN_NAME='AadhaarNo') ALTER TABLE [dbo].[Helpers] ADD [AadhaarNo] NVARCHAR(20) NULL",
    };
    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        int applied = 0;
        foreach (var sql in statements)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            applied++;
        }
        Console.WriteLine($"[Migration] Helpers ID-card columns migration complete ({applied} statements executed).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Migration] Helpers WARNING: {ex.Message}");
    }
}

// ────────────────────────────────────────────────────────────────────────────────
// Applies GPS check-in / check-out / approval columns to [Attendance].
// Every statement is wrapped in IF NOT EXISTS so it is safe to run on every boot.
// Uses raw ADO.NET because EF Core does not support the GO batch separator.
// ────────────────────────────────────────────────────────────────────────────────
static void RunAttendanceMigrations(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("[Migration] Skipped — connection string is empty.");
        return;
    }

    // Each entry is one idempotent T-SQL statement (no GO separators needed).
    var statements = new[]
    {
        // ── 001: GPS Check-In columns ─────────────────────────────────────────
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='Latitude') ALTER TABLE [dbo].[Attendance] ADD [Latitude] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='Longitude') ALTER TABLE [dbo].[Attendance] ADD [Longitude] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='GpsAccuracy') ALTER TABLE [dbo].[Attendance] ADD [GpsAccuracy] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckInTime') ALTER TABLE [dbo].[Attendance] ADD [CheckInTime] DATETIME NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='Address') ALTER TABLE [dbo].[Attendance] ADD [Address] NVARCHAR(MAX) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='Status') ALTER TABLE [dbo].[Attendance] ADD [Status] NVARCHAR(50) NULL",

        // ── 002: GPS Check-Out columns ────────────────────────────────────────
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckOutTime') ALTER TABLE [dbo].[Attendance] ADD [CheckOutTime] DATETIME NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckOutLatitude') ALTER TABLE [dbo].[Attendance] ADD [CheckOutLatitude] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckOutLongitude') ALTER TABLE [dbo].[Attendance] ADD [CheckOutLongitude] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckOutGpsAccuracy') ALTER TABLE [dbo].[Attendance] ADD [CheckOutGpsAccuracy] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='CheckOutAddress') ALTER TABLE [dbo].[Attendance] ADD [CheckOutAddress] NVARCHAR(MAX) NULL",

        // ── 002: Manager Approval columns ─────────────────────────────────────
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='TotalHours') ALTER TABLE [dbo].[Attendance] ADD [TotalHours] FLOAT NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='ManagerRemarks') ALTER TABLE [dbo].[Attendance] ADD [ManagerRemarks] NVARCHAR(MAX) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='ApprovedBy') ALTER TABLE [dbo].[Attendance] ADD [ApprovedBy] NVARCHAR(100) NULL",
        "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='ApprovalTimestamp') ALTER TABLE [dbo].[Attendance] ADD [ApprovalTimestamp] DATETIME NULL",

        // ── 002: Convert legacy Time column TIME(7) → FLOAT if needed ─────────
        @"IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME='Attendance' AND COLUMN_NAME='Time' AND DATA_TYPE='time')
          BEGIN
              UPDATE [dbo].[Attendance] SET [Time] = NULL WHERE [Time] IS NOT NULL;
              ALTER TABLE [dbo].[Attendance] ALTER COLUMN [Time] FLOAT NULL;
          END"
    };

    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        int applied = 0;
        foreach (var sql in statements)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            applied++;
        }
        Console.WriteLine($"[Migration] Attendance schema migration complete ({applied} statements executed).");
    }
    catch (Exception ex)
    {
        // Log but do not crash startup — the app can still serve pages that don't
        // touch the new columns while the admin investigates the DB connection.
        Console.WriteLine($"[Migration] WARNING: {ex.Message}");
    }
}
