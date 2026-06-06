using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployeeRemove;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployeeRemove;

public class EmployeeRemoveService : IEmployeeRemoveService
{
    private readonly AppDbContext _dbContext;

    public EmployeeRemoveService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> RemoveEmployeeAsync(EmployeeRemoveDto dto)
{
    var employee = await _dbContext.Employees
        .FirstOrDefaultAsync(e =>
            e.RpNumber == dto.RpNumber &&
            e.EmployeeNumber == dto.EmployeeNumber);

    if (employee == null)
        throw new Exception("Employee not found");

    _dbContext.Employees.Remove(employee);
    await _dbContext.SaveChangesAsync();

    return employee.Id;
}
}