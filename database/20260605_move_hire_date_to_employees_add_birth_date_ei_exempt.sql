begin;

alter table public."Employees"
    add column if not exists "Hire Date" date;

alter table public."Employees"
    add column if not exists "Date of Birth" date;

alter table public."Employees"
    add column if not exists "EI_Exempt" boolean not null default false;

update public."Employees" as e
set "Hire Date" = pr."Hire Date"
from (
    select "Employee #", min("Hire Date") as "Hire Date"
    from public."PayrollRecords"
    where "Hire Date" is not null
    group by "Employee #"
) as pr
where e."Employee #" = pr."Employee #"
  and e."Hire Date" is null;

alter table public."PayrollRecords"
    drop column if exists "Hire Date";

commit;
