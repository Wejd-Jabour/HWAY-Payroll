namespace HWAY.Payroll.Services.PayrollHistory;

public interface IPayrollHistoryLookupService
{
    Task<List<PayrollHistoryTermOptionDto>> GetTermOptionsAsync(string rpNumber);

    Task<List<int>> GetYearsAsync(string rpNumber, string? employerGroup);

    Task<List<PayrollHistoryMonthOptionDto>> GetMonthsAsync(string rpNumber, string? employerGroup, int year);

    Task<List<int>> GetPeriodsAsync(string rpNumber, string? employerGroup, int year, int? month);

    Task<PayrollHistorySearchResultDto> SearchAsync(PayrollHistorySearchRequest request);
}
