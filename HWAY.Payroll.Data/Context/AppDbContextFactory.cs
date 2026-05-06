using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HWAY.Payroll.Data.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

    optionsBuilder.UseSqlServer(
        @"Server=tcp:hway.database.windows.net,1433;
        Initial Catalog=HWay_Payroll;
        Persist Security Info=False;
        User ID=jabourw;
        Password=HwayPayroll1;
        MultipleActiveResultSets=False;
        Encrypt=True;
        TrustServerCertificate=False;
        Connection Timeout=30;");

        return new AppDbContext(optionsBuilder.Options);
    }
}