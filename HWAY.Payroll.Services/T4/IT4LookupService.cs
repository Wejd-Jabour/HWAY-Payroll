namespace HWAY.Payroll.Services.T4;

public interface IT4LookupService
{
    Task<T4CompanyDto> CalculateCompanyT4Async(string rpNumber, int year);
}
