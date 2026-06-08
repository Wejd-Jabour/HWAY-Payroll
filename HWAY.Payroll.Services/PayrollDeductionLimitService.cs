using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.T4;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services;

public sealed class PayrollDeductionLimitService : IPayrollDeductionLimitService
{
    private readonly AppDbContext _dbContext;

    public PayrollDeductionLimitService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PayrollDeductionLimitDto>> GetLimitsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultLimitsAsync(cancellationToken);

        return await _dbContext.PayrollDeductionLimits
            .AsNoTracking()
            .OrderByDescending(limit => limit.Year)
            .Select(limit => new PayrollDeductionLimitDto
            {
                Year = limit.Year,
                Cpp1Maximum = limit.Cpp1Maximum,
                Cpp2Maximum = limit.Cpp2Maximum,
                EiMaximum = limit.EiMaximum,
                CreatedAt = limit.CreatedAt,
                UpdatedAt = limit.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollDeductionLimitValues?> GetLimitValuesForYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultLimitsAsync(cancellationToken);

        var limit = await _dbContext.PayrollDeductionLimits
            .AsNoTracking()
            .Where(item => item.Year == year)
            .Select(item => new PayrollDeductionLimitValues(
                item.Year,
                item.Cpp1Maximum,
                item.Cpp2Maximum,
                item.EiMaximum))
            .FirstOrDefaultAsync(cancellationToken);

        return limit ?? PayrollDeductionCaps.GetDefaultLimits(year);
    }

    public async Task SaveLimitAsync(
        PayrollDeductionLimitDto limit,
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultLimitsAsync(cancellationToken);
        Validate(limit);

        var now = DateTime.UtcNow;
        var entity = await _dbContext.PayrollDeductionLimits
            .FirstOrDefaultAsync(item => item.Year == limit.Year, cancellationToken);

        if (entity is null)
        {
            entity = new PayrollDeductionLimit
            {
                Year = limit.Year,
                CreatedAt = now
            };

            _dbContext.PayrollDeductionLimits.Add(entity);
        }

        entity.Cpp1Maximum = limit.Cpp1Maximum;
        entity.Cpp2Maximum = limit.Cpp2Maximum;
        entity.EiMaximum = limit.EiMaximum;
        entity.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureDefaultLimitsAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            create table if not exists public."PayrollDeductionLimits" (
                "Year" integer not null,
                "CPP1Maximum" numeric(18,2) not null,
                "CPP2Maximum" numeric(18,2) not null,
                "EIMaximum" numeric(18,2) not null,
                "CreatedAt" timestamp with time zone not null default now(),
                "UpdatedAt" timestamp with time zone not null default now(),
                constraint "PK_PayrollDeductionLimits" primary key ("Year")
            );
            """,
            cancellationToken);

        foreach (var year in T4TaxYearRuleProvider.SupportedYears)
        {
            var exists = await _dbContext.PayrollDeductionLimits
                .AnyAsync(limit => limit.Year == year, cancellationToken);

            if (exists)
            {
                continue;
            }

            var rules = T4TaxYearRuleProvider.GetRules(year);

            if (rules is null)
            {
                continue;
            }

            var now = DateTime.UtcNow;
            _dbContext.PayrollDeductionLimits.Add(new PayrollDeductionLimit
            {
                Year = year,
                Cpp1Maximum = rules.Cpp1MaximumContribution,
                Cpp2Maximum = rules.Cpp2MaximumContribution,
                EiMaximum = rules.EiMaximumPremium,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(PayrollDeductionLimitDto limit)
    {
        if (limit.Year < 2000)
        {
            throw new InvalidOperationException("Year must be 2000 or later.");
        }

        if (limit.Cpp1Maximum < 0)
        {
            throw new InvalidOperationException("CPP1 maximum cannot be negative.");
        }

        if (limit.Cpp2Maximum < 0)
        {
            throw new InvalidOperationException("CPP2 maximum cannot be negative.");
        }

        if (limit.EiMaximum < 0)
        {
            throw new InvalidOperationException("EI maximum cannot be negative.");
        }
    }
}
