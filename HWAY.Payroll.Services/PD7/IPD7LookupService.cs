using System.Collections.Generic;
using System.Threading.Tasks;

namespace HWAY.Payroll.Services.PD7;

public interface IPD7LookupService
{
    Task<List<PD7LookupDto>> SearchPd7Async(string? searchText);
    Task<PD7LookupDto?> GetByRpNumberAsync(string rpNumber);
    Task<PD7LookupDto?> GetByPeriodAsync(string rpNumber, int month, int year);
    Task<decimal> GetCalendarYearPd7TotalAsync(string rpNumber, int year);
    Task<PD7LookupDto> CalculatePd7Async(string rpNumber, int month, int year);
    Task<PD7LookupDto> SaveAutomaticPd7Async(string rpNumber, int month, int year);
    Task<bool> CreatePd7Async(PD7LookupDto dto);
}
