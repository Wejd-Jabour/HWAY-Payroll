-- Add the employer-level paystub period tracker.
-- New employers start at period 1; existing employers are initialized to 1.

begin;

alter table public."Employers"
    add column if not exists "Period #" integer not null default 1;

update public."Employers"
set "Period #" = 1
where "Period #" is null
   or "Period #" < 1;

alter table public."Employers"
    alter column "Period #" set default 1,
    alter column "Period #" set not null;

commit;
