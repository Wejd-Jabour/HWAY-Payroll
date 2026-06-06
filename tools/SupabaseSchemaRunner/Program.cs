using Npgsql;

const string SupabaseProjectRef = "bgobasrlbmxnzflnfzch";

var connectionString = GetSupabaseConnectionString();

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set HWAY_PAYROLL_DB_CONNECTION, SUPABASE_CONNECTION_STRING, or SUPABASE_DB_PASSWORD before running this tool.");
}

var resetSchema = args.Contains("--reset", StringComparer.OrdinalIgnoreCase);
var verifyOnly = args.Contains("--verify-only", StringComparer.OrdinalIgnoreCase);
var schemaArgument = args.FirstOrDefault(arg => !arg.StartsWith("--", StringComparison.Ordinal));

var schemaPath = schemaArgument is not null
    ? Path.GetFullPath(schemaArgument)
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "supabase_schema.sql"));

var schemaSql = verifyOnly
    ? string.Empty
    : schemaArgument == "-"
        ? await Console.In.ReadToEndAsync()
        : await File.ReadAllTextAsync(schemaPath);
var resetSql = """
drop table if exists public."bi-weekly B" cascade;
drop table if exists public."bi-weekly A" cascade;
drop table if exists public."semi-monthly" cascade;
drop table if exists public."monthly" cascade;
drop table if exists public."Record History" cascade;
drop table if exists public."PayrollRecords" cascade;
drop table if exists public."PD7" cascade;
drop table if exists public."Employees" cascade;
drop table if exists public."Employers" cascade;
""";

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

if (resetSchema)
{
    await using var resetCommand = new NpgsqlCommand(resetSql, connection);
    resetCommand.CommandTimeout = 120;
    await resetCommand.ExecuteNonQueryAsync();
}

if (!verifyOnly)
{
    await using var schemaCommand = new NpgsqlCommand(schemaSql, connection);
    schemaCommand.CommandTimeout = 120;
    await schemaCommand.ExecuteNonQueryAsync();
}

const string verificationSql = """
select
    c.table_name,
    count(*) as column_count
from information_schema.columns c
where c.table_schema = 'public'
  and c.table_name in (
      'Employees',
      'Employers',
      'PayrollRecords',
      'Record History',
      'PD7',
      'monthly',
      'semi-monthly',
      'bi-weekly A',
      'bi-weekly B'
  )
group by c.table_name
order by c.table_name;
""";

await using var verificationCommand = new NpgsqlCommand(verificationSql, connection);
await using var reader = await verificationCommand.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    Console.WriteLine($"{reader.GetString(0)}: {reader.GetInt64(1)} columns");
}

await reader.CloseAsync();

const string periodColumnVerificationSql = """
select
    c.table_name,
    count(*) filter (where c.column_name ~ '^P[0-9]+$') as period_column_count,
    count(*) filter (where c.column_name ~ '^P[0-9]+$' and c.data_type = 'date') as date_period_column_count,
    count(*) filter (where c.column_name in ('M1', 'M2') and c.data_type = 'numeric') as numeric_marker_column_count
from information_schema.columns c
where c.table_schema = 'public'
  and c.table_name in ('monthly', 'semi-monthly', 'bi-weekly A', 'bi-weekly B')
group by c.table_name
order by c.table_name;
""";

await using var periodColumnVerificationCommand = new NpgsqlCommand(periodColumnVerificationSql, connection);
await using var periodColumnVerificationReader = await periodColumnVerificationCommand.ExecuteReaderAsync();

while (await periodColumnVerificationReader.ReadAsync())
{
    Console.WriteLine(
        $"{periodColumnVerificationReader.GetString(0)}: " +
        $"{periodColumnVerificationReader.GetInt64(2)}/{periodColumnVerificationReader.GetInt64(1)} period date columns, " +
        $"{periodColumnVerificationReader.GetInt64(3)} numeric marker columns");
}

await periodColumnVerificationReader.CloseAsync();

const string rowCountSql = """
select 'Employees' as table_name, count(*) as row_count from public."Employees"
union all
select 'Employers' as table_name, count(*) as row_count from public."Employers"
union all
select 'PD7' as table_name, count(*) as row_count from public."PD7"
union all
select 'PayrollRecords' as table_name, count(*) as row_count from public."PayrollRecords"
union all
select 'Record History' as table_name, count(*) as row_count from public."Record History"
union all
select 'monthly' as table_name, count(*) as row_count from public."monthly"
union all
select 'semi-monthly' as table_name, count(*) as row_count from public."semi-monthly"
union all
select 'bi-weekly A' as table_name, count(*) as row_count from public."bi-weekly A"
union all
select 'bi-weekly B' as table_name, count(*) as row_count from public."bi-weekly B"
order by table_name;
""";

await using var rowCountCommand = new NpgsqlCommand(rowCountSql, connection);
await using var rowCountReader = await rowCountCommand.ExecuteReaderAsync();

while (await rowCountReader.ReadAsync())
{
    Console.WriteLine($"{rowCountReader.GetString(0)}: {rowCountReader.GetInt64(1)} rows");
}

await rowCountReader.CloseAsync();

const string scheduleSummarySql = """
select
    'monthly' as table_name,
    "P1" as first_period_date,
    "P12" as last_period_date
from public."monthly"
where "ID" = 1
union all
select
    'semi-monthly' as table_name,
    "P1" as first_period_date,
    "P24" as last_period_date
from public."semi-monthly"
where "ID" = 1
union all
select
    'bi-weekly A' as table_name,
    "P1" as first_period_date,
    "P26" as last_period_date
from public."bi-weekly A"
where "ID" = 1
union all
select
    'bi-weekly B' as table_name,
    "P1" as first_period_date,
    "P26" as last_period_date
from public."bi-weekly B"
where "ID" = 1
order by table_name;
""";

await using var scheduleSummaryCommand = new NpgsqlCommand(scheduleSummarySql, connection);
await using var scheduleSummaryReader = await scheduleSummaryCommand.ExecuteReaderAsync();

while (await scheduleSummaryReader.ReadAsync())
{
    Console.WriteLine(
        $"{scheduleSummaryReader.GetString(0)} ID 1: " +
        $"{scheduleSummaryReader.GetDateTime(1):yyyy-MM-dd} to {scheduleSummaryReader.GetDateTime(2):yyyy-MM-dd}");
}

static string? GetSupabaseConnectionString()
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
        return null;
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

static string? GetEnvironmentVariable(string name)
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

static string? TryGetEnvironmentVariable(string name, EnvironmentVariableTarget target)
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

static string NormalizeConnectionString(string connectionString)
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
