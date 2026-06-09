namespace HWAY.Payroll.Services.YearEnd;

public interface IYearEndResetService
{
    Task<YearEndResetResult> ResetYearAsync(
        int year,
        CancellationToken cancellationToken = default);
}

public sealed record YearEndResetResult(
    int Year,
    int PayrollRecordsReset,
    int EmployersReset);
