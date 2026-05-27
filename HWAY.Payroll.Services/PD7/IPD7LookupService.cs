using System.Collections.Generic;
using System.Threading.Tasks;

namespace HWAY.Payroll.Services.PD7;

public interface IPD7LookupService
{
    Task<List<PD7LookupDto>> SearchPd7Async(string? searchText);
    Task<PD7LookupDto?> GetByRpNumberAsync(string rpNumber);
}
