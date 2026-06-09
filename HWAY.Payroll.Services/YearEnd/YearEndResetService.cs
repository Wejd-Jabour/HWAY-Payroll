using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.YearEnd;

public sealed class YearEndResetService : IYearEndResetService
{
    private readonly AppDbContext _dbContext;

    public YearEndResetService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<YearEndResetResult> ResetYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        if (year < 1)
        {
            throw new InvalidOperationException("Year is required.");
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var payrollRecordsReset = await _dbContext.PayrollRecords
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(record => record.Period, 0)
                    .SetProperty(record => record.RegularHours, 0m)
                    .SetProperty(record => record.OvertimeHours, 0m)
                    .SetProperty(record => record.PublicHolidayHours, 0m)
                    .SetProperty(record => record.StatHolidayHours, 0m)
                    .SetProperty(record => record.Vacation, 0m)
                    .SetProperty(record => record.Gross, 0m)
                    .SetProperty(record => record.Tax1, 0m)
                    .SetProperty(record => record.Cpp1, 0m)
                    .SetProperty(record => record.Cpp2, 0m)
                    .SetProperty(record => record.Ei, 0m)
                    .SetProperty(record => record.OtherDeduction, 0m)
                    .SetProperty(record => record.NetPay, 0m)
                    .SetProperty(record => record.GrossYtd, 0m)
                    .SetProperty(record => record.NetYtd, 0m)
                    .SetProperty(record => record.TaxYtd, 0m)
                    .SetProperty(record => record.Cpp1Ytd, 0m)
                    .SetProperty(record => record.Cpp2Ytd, 0m)
                    .SetProperty(record => record.EiYtd, 0m)
                    .SetProperty(record => record.OtherDeductionYtd, 0m)
                    .SetProperty(record => record.VacationYtd, 0m)
                    .SetProperty(record => record.VacationYtdUsed, 0m)
                    .SetProperty(record => record.VacationYtdBalance, 0m)
                    .SetProperty(record => record.TotalPd7, 0m),
                    cancellationToken);

            var employersReset = await _dbContext.Employers
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(employer => employer.PaystubPeriod, 1),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new YearEndResetResult(
                year,
                payrollRecordsReset,
                employersReset);
        });
    }
}
