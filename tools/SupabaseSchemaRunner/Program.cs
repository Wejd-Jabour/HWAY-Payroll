using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Set SUPABASE_CONNECTION_STRING before running this tool.");
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
  and c.table_name in ('Employees', 'Employers', 'PayrollRecords', 'PD7')
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

const string rowCountSql = """
select 'Employees' as table_name, count(*) as row_count from public."Employees"
union all
select 'Employers' as table_name, count(*) as row_count from public."Employers"
union all
select 'PD7' as table_name, count(*) as row_count from public."PD7"
union all
select 'PayrollRecords' as table_name, count(*) as row_count from public."PayrollRecords"
order by table_name;
""";

await using var rowCountCommand = new NpgsqlCommand(rowCountSql, connection);
await using var rowCountReader = await rowCountCommand.ExecuteReaderAsync();

while (await rowCountReader.ReadAsync())
{
    Console.WriteLine($"{rowCountReader.GetString(0)}: {rowCountReader.GetInt64(1)} rows");
}
