begin;

create table if not exists public."PayrollDeductionLimits" (
    "Year" integer not null,
    "CPP1Maximum" numeric(18,2) not null,
    "CPP2Maximum" numeric(18,2) not null,
    "EIMaximum" numeric(18,2) not null,
    "CreatedAt" timestamp with time zone not null default now(),
    "UpdatedAt" timestamp with time zone not null default now(),
    constraint "PK_PayrollDeductionLimits" primary key ("Year")
);

insert into public."PayrollDeductionLimits" (
    "Year",
    "CPP1Maximum",
    "CPP2Maximum",
    "EIMaximum"
)
values
    (2024, 3867.50, 188.00, 1049.12),
    (2025, 4034.10, 396.00, 1077.48),
    (2026, 4230.45, 416.00, 1123.07)
on conflict ("Year") do nothing;

commit;
