-- Convert payroll period columns from numeric values to dates.
-- M1 and M2 stay numeric on the bi-weekly tables.

begin;

do $$
declare
    period_column record;
begin
    for period_column in
        select
            c.table_name,
            c.column_name,
            c.data_type
        from information_schema.columns c
        where c.table_schema = 'public'
          and c.table_name in ('monthly', 'semi-monthly', 'bi-weekly A', 'bi-weekly B')
          and c.column_name ~ '^P[0-9]+$'
        order by c.table_name, c.ordinal_position
    loop
        execute format(
            'alter table public.%I alter column %I drop default',
            period_column.table_name,
            period_column.column_name);

        execute format(
            'alter table public.%I alter column %I drop not null',
            period_column.table_name,
            period_column.column_name);

        if period_column.data_type <> 'date' then
            execute format(
                'alter table public.%I alter column %I type date using null',
                period_column.table_name,
                period_column.column_name);
        end if;
    end loop;
end $$;

do $$
declare
    mismatch_count integer;
begin
    select count(*)
    into mismatch_count
    from information_schema.columns c
    where c.table_schema = 'public'
      and c.table_name in ('monthly', 'semi-monthly', 'bi-weekly A', 'bi-weekly B')
      and c.column_name ~ '^P[0-9]+$'
      and c.data_type <> 'date';

    if mismatch_count <> 0 then
        raise exception 'Not all payroll period columns were converted to date columns.';
    end if;
end $$;

commit;
