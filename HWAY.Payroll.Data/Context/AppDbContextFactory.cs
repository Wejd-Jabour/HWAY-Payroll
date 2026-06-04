using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HWAY.Payroll.Data.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        DatabaseConfiguration.ConfigureSupabase(optionsBuilder);

        return new AppDbContext(optionsBuilder.Options);
    }
}
