using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployerRemove;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployerRemove;

public class EmployerRemoveService : IEmployerRemoveService
{
    private readonly AppDbContext _dbContext;

    public EmployerRemoveService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> RemoveEmployerAsync(EmployerRemoveDto dto)
{
    var employer = await _dbContext.Employers
        .FirstOrDefaultAsync(e =>
            e.RpNumber == dto.RpNumber &&
            e.EmployerName == dto.EmployerName);

    if (employer == null)
        throw new Exception("Employer not found");

    _dbContext.Employers.Remove(employer);
    await _dbContext.SaveChangesAsync();

    return employer.Id;
}
}