using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HWAY.Payroll.Data.Context;

public static class DatabaseConfiguration
{
    private const string SupabaseProjectRef = "bgobasrlbmxnzflnfzch";
    private const string BuiltInSupabaseConnectionString =
        "Host=aws-1-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.bgobasrlbmxnzflnfzch;Password=Hway2026$$$;Ssl Mode=Require;Timeout=30;Command Timeout=120";

    public static void ConfigureSupabase(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            GetSupabaseConnectionString(),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null));
    }

    public static string GetSupabaseConnectionString()
    {
        var configuredConnectionString =
            GetEnvironmentVariable("HWAY_PAYROLL_DB_CONNECTION")
            ?? GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return NormalizeConnectionString(configuredConnectionString);
        }

        var password = GetEnvironmentVariable("SUPABASE_DB_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
        {
            return NormalizeConnectionString(BuiltInSupabaseConnectionString);
        }

        var host = GetEnvironmentVariable("SUPABASE_DB_HOST")
            ?? $"db.{SupabaseProjectRef}.supabase.co";

        var username = GetEnvironmentVariable("SUPABASE_DB_USER") ?? "postgres";
        var database = GetEnvironmentVariable("SUPABASE_DB_NAME") ?? "postgres";
        var portValue = GetEnvironmentVariable("SUPABASE_DB_PORT");
        var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 5432;

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
            Timeout = 30,
            CommandTimeout = 30
        };

        return connectionStringBuilder.ConnectionString;
    }

    private static string? GetEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = TryGetEnvironmentVariable(name, EnvironmentVariableTarget.User);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return TryGetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
    }

    private static string? TryGetEnvironmentVariable(string name, EnvironmentVariableTarget target)
    {
        try
        {
            return Environment.GetEnvironmentVariable(name, target);
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.TrimStart('/');

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = string.IsNullOrWhiteSpace(database) ? "postgres" : Uri.UnescapeDataString(database),
            Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "postgres",
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = SslMode.Require,
            Timeout = 30,
            CommandTimeout = 30
        };

        return connectionStringBuilder.ConnectionString;
    }
}
