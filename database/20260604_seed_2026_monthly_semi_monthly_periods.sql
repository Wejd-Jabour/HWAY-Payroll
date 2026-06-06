-- Seed 2026 monthly and semi-monthly payroll period start dates.
-- Rerunning this script updates row ID 1 instead of creating duplicate rows.

begin;

insert into public."monthly" (
    "ID",
    "P1",
    "P2",
    "P3",
    "P4",
    "P5",
    "P6",
    "P7",
    "P8",
    "P9",
    "P10",
    "P11",
    "P12"
)
values (
    1,
    date '2026-01-01',
    date '2026-02-01',
    date '2026-03-01',
    date '2026-04-01',
    date '2026-05-01',
    date '2026-06-01',
    date '2026-07-01',
    date '2026-08-01',
    date '2026-09-01',
    date '2026-10-01',
    date '2026-11-01',
    date '2026-12-01'
)
on conflict ("ID") do update set
    "P1" = excluded."P1",
    "P2" = excluded."P2",
    "P3" = excluded."P3",
    "P4" = excluded."P4",
    "P5" = excluded."P5",
    "P6" = excluded."P6",
    "P7" = excluded."P7",
    "P8" = excluded."P8",
    "P9" = excluded."P9",
    "P10" = excluded."P10",
    "P11" = excluded."P11",
    "P12" = excluded."P12";

insert into public."semi-monthly" (
    "ID",
    "P1",
    "P2",
    "P3",
    "P4",
    "P5",
    "P6",
    "P7",
    "P8",
    "P9",
    "P10",
    "P11",
    "P12",
    "P13",
    "P14",
    "P15",
    "P16",
    "P17",
    "P18",
    "P19",
    "P20",
    "P21",
    "P22",
    "P23",
    "P24"
)
values (
    1,
    date '2026-01-01',
    date '2026-01-16',
    date '2026-02-01',
    date '2026-02-16',
    date '2026-03-01',
    date '2026-03-16',
    date '2026-04-01',
    date '2026-04-16',
    date '2026-05-01',
    date '2026-05-16',
    date '2026-06-01',
    date '2026-06-16',
    date '2026-07-01',
    date '2026-07-16',
    date '2026-08-01',
    date '2026-08-16',
    date '2026-09-01',
    date '2026-09-16',
    date '2026-10-01',
    date '2026-10-16',
    date '2026-11-01',
    date '2026-11-16',
    date '2026-12-01',
    date '2026-12-16'
)
on conflict ("ID") do update set
    "P1" = excluded."P1",
    "P2" = excluded."P2",
    "P3" = excluded."P3",
    "P4" = excluded."P4",
    "P5" = excluded."P5",
    "P6" = excluded."P6",
    "P7" = excluded."P7",
    "P8" = excluded."P8",
    "P9" = excluded."P9",
    "P10" = excluded."P10",
    "P11" = excluded."P11",
    "P12" = excluded."P12",
    "P13" = excluded."P13",
    "P14" = excluded."P14",
    "P15" = excluded."P15",
    "P16" = excluded."P16",
    "P17" = excluded."P17",
    "P18" = excluded."P18",
    "P19" = excluded."P19",
    "P20" = excluded."P20",
    "P21" = excluded."P21",
    "P22" = excluded."P22",
    "P23" = excluded."P23",
    "P24" = excluded."P24";

select setval(
    pg_get_serial_sequence('public."monthly"', 'ID'),
    coalesce((select max("ID") from public."monthly"), 1),
    true);

select setval(
    pg_get_serial_sequence('public."semi-monthly"', 'ID'),
    coalesce((select max("ID") from public."semi-monthly"), 1),
    true);

commit;
