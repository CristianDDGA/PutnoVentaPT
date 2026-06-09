using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: dotnet run -- <connection-string>");
            return 2;
        }

        var connString = args[0];
        string[] tables = new[] { "Customers", "Products", "Roles", "Users", "Sales", "SaleDetails", "StockMovements", "ErrorLogs", "__EFMigrationsHistory" };

        try
        {
            using var conn = new SqlConnection(connString);
            Console.WriteLine($"Connecting to: {conn.DataSource} -> Database: {conn.Database}");
            await conn.OpenAsync();
            Console.WriteLine("Connected.");

            foreach (var t in tables)
            {
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT COUNT(*) FROM dbo.[{t}]";
                    var result = await cmd.ExecuteScalarAsync();
                    Console.WriteLine($"{t}: {result}");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"{t}: ERROR - {ex.Message}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Connection failed: " + ex.Message);
            return 1;
        }
    }
}
