namespace HWAY.Payroll.Services;

public interface IPayrollDeductionLimitService
{
    Task<IReadOnlyList<PayrollDeductionLimitDto>> GetLimitsAsync(
        CancellationToken cancellationToken = default);

    Task<PayrollDeductionLimitValues?> GetLimitValuesForYearAsync(
        int year,
        CancellationToken cancellationToken = default);

    Task SaveLimitAsync(
        PayrollDeductionLimitDto limit,
        CancellationToken cancellationToken = default);

    Task EnsureDefaultLimitsAsync(CancellationToken cancellationToken = default);
}
