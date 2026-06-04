# Supabase Migration

The app now uses Supabase Postgres through EF Core/Npgsql.

## Project

- REST URL: `https://bgobasrlbmxnzflnfzch.supabase.co/rest/v1/`
- Project ref: `bgobasrlbmxnzflnfzch`

The publishable API key is not enough to create tables or run this EF Core app. Use the Supabase database password or the full Postgres connection string.

The direct database host for this project is IPv6-only. On this machine, use the Supabase **Session Pooler** connection string unless IPv6 is available.

## Configure The App

Set one of these before running the app:

```powershell
$env:SUPABASE_DB_PASSWORD = "<supabase database password>"
```

or:

```powershell
$env:SUPABASE_CONNECTION_STRING = "Host=db.bgobasrlbmxnzflnfzch.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<supabase database password>;Ssl Mode=Require;Trust Server Certificate=true"
```

Supabase `postgresql://...` connection strings are also accepted.

The app also accepts `HWAY_PAYROLL_DB_CONNECTION` if you prefer a project-specific variable.

## Create Tables

Run `database/supabase_schema.sql` in the Supabase SQL editor. It creates:

- `Employees`
- `Employers`
- `PayrollRecords`
- `PD7`

The script preserves the existing table names, quoted column names, decimals, indexes, foreign keys, and the current `PD7` composite key.

Do not run the existing EF migration history against Supabase. Those migration files were generated for SQL Server and are retained as project history; use `database/supabase_schema.sql` for the Supabase database.

## Data Migration

The existing Azure database could not be read from this machine because SQL Server returned:

```text
Can not connect to the database in its current state.
```

Once Azure is reachable, export/import in dependency order:

1. `Employees`
2. `Employers`
3. `PayrollRecords`
4. `PD7`

Keep identity values for `Id` columns during import, then reset each identity sequence to `max("Id")`.
