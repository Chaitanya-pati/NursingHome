using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

const string connStr = "Server=sql.bsite.net\\MSSQL2016;Database=subrmanyahealthcare_;MultipleActiveResultSets=True;User Id=subrmanyahealthcare_;Password=Vicky@123;TrustServerCertificate=True;";

string[] migrationFiles = new[]
{
    "../DBSchema/Migrations/003_AddHelperIDCardFields.sql"
};

Console.WriteLine("Connecting to remote SQL Server...");
using var conn = new SqlConnection(connStr);
conn.Open();
Console.WriteLine("Connected.\n");

foreach (var file in migrationFiles)
{
    Console.WriteLine($"Running migration: {Path.GetFileName(file)}");
    string sql = File.ReadAllText(file);
    var batches = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
    int batchNum = 0;
    foreach (var batch in batches)
    {
        string trimmed = batch.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) continue;
        batchNum++;
        using var cmd = new SqlCommand(trimmed, conn);
        cmd.CommandTimeout = 60;
        try
        {
            cmd.ExecuteNonQuery();
            Console.WriteLine($"  ✓ Batch {batchNum} OK");
        }
        catch (SqlException ex) when (
            ex.Message.Contains("Column already exists") ||
            ex.Message.Contains("already an object") ||
            ex.Message.Contains("Column names in each table must be unique"))
        {
            Console.WriteLine($"  ~ Batch {batchNum} skipped (already applied)");
        }
    }
    Console.WriteLine($"  Done: {Path.GetFileName(file)}\n");
}
Console.WriteLine("All migrations applied successfully.");
