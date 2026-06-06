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
- `Record History`
- `PD7`
- `monthly`
- `semi-monthly`
- `bi-weekly A`
- `bi-weekly B`

The script preserves the existing table names, quoted column names, decimals, indexes, foreign keys, and the current `PD7` composite key. The payroll period table `P*` columns are period start date columns; `M1` and `M2` on the bi-weekly tables remain numeric.

For an existing Supabase database that already has the base schema, run `database/20260604_create_pay_period_tables.sql` to add only the new payroll period tables.

For an existing Supabase database, run `database/20260604_add_employer_paystub_period.sql` to add the employer-level `Period #` tracker.

For an existing Supabase database, run `database/20260606_create_record_history_table.sql` to add the passive `Record History` table populated by the Save All Employees flow.

If the payroll period tables were created before the `P*` columns were corrected to dates, run `database/20260604_convert_pay_period_columns_to_date.sql`.

Run `database/20260604_seed_2026_monthly_semi_monthly_periods.sql` to populate row `ID = 1` in `monthly` and `semi-monthly` with the 2026 payroll period start dates. Monthly periods use the first day of each month. Semi-monthly periods alternate the 1st and 16th.

Run `database/20260604_seed_2026_bi_weekly_periods.sql` to populate row `ID = 1` in `bi-weekly A` and `bi-weekly B` with the period start dates from the supplied schedules.

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
4. `Record History`
5. `PD7`

Keep identity values for `Id` columns during import, then reset each identity sequence to `max("Id")`.
