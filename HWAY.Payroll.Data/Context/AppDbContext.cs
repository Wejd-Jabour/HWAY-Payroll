using HWAY.Payroll.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<PD7> PD7Records => Set<PD7>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employer>(entity =>
        {
            entity.ToTable("Employers");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.RpNumber)
                .HasColumnName("RP#")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.EmployerName)
                .HasColumnName("Employer Name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);

            entity.Property(e => e.PostalCode)
                .HasColumnName("Postal code")
                .HasMaxLength(20);

            entity.Property(e => e.EmployerGroup)
                .HasColumnName("Employer Group")
                .HasMaxLength(100);

            entity.Property(e => e.PaystubPeriod)
                .HasColumnName("Period #")
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.Fee).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.RpNumber).IsUnique();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeNumber)
                .HasColumnName("Employee #")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.FirstName)
                .HasColumnName("First Name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasColumnName("Last Name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Sin)
                .HasColumnName("SIN")
                .HasMaxLength(20);

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(50);

            entity.Property(e => e.PostalCode)
                .HasColumnName("Postal Code")
                .HasMaxLength(20);

            entity.Property(e => e.EmailAddress)
                .HasColumnName("E-Mail Address")
                .HasMaxLength(200);

            entity.Property(e => e.HireDate)
                .HasColumnName("Hire Date")
                .HasColumnType("date");

            entity.Property(e => e.DateOfBirth)
                .HasColumnName("Date of Birth")
                .HasColumnType("date");

            entity.Property(e => e.EiExempt)
                .HasColumnName("EI_Exempt")
                .HasDefaultValue(false);

            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
        });

        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.ToTable("PayrollRecords");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeNumber).HasColumnName("Employee #").HasMaxLength(50).IsRequired();
            entity.Property(e => e.RpNumber).HasColumnName("RP#").HasMaxLength(50).IsRequired();
            entity.Property(e => e.EmployeeName).HasColumnName("Employees Name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Period)
                .HasColumnName("Period")
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Rate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RegularHours).HasColumnName("R.Hours").HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimeHours).HasColumnName("OV.Hours").HasColumnType("decimal(18,2)");
            entity.Property(e => e.PublicHolidayHours).HasColumnName("Public.H").HasColumnType("decimal(18,2)");
            entity.Property(e => e.StatHolidayHours).HasColumnName("S.H Hours").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Vacation).HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacationSetting)
                .HasColumnName("vacation_setting")
                .HasMaxLength(20)
                .HasDefaultValue(HWAY.Payroll.Domain.VacationSettings.Accumulate)
                .IsRequired();
            entity.Property(e => e.Gross).HasColumnType("decimal(18,2)");

            entity.Property(e => e.Tax1).HasColumnName("TAX1").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cpp1).HasColumnName("CPP1").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cpp2).HasColumnName("CPP2").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Ei).HasColumnName("EI").HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherDeduction).HasColumnName("Other deduction").HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetPay).HasColumnName("NET PAY").HasColumnType("decimal(18,2)");

            entity.Property(e => e.GrossYtd).HasColumnName("Gross-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetYtd).HasColumnName("NET-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxYtd).HasColumnName("TAX -YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cpp1Ytd).HasColumnName("CPP1-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cpp2Ytd).HasColumnName("CPP2-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.EiYtd).HasColumnName("EI-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherDeductionYtd).HasColumnName("Other Deduction-YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacationYtd).HasColumnName("Vacation -YTD").HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacationYtdUsed).HasColumnName("Vacation -YTD-Used").HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacationYtdBalance).HasColumnName("Vacation -YTD- Balance").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPd7).HasColumnName("Total PD7").HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Employee)
                .WithMany(e => e.PayrollRecords)
                .HasPrincipalKey(e => e.EmployeeNumber)
                .HasForeignKey(e => e.EmployeeNumber);

            entity.HasOne(e => e.Employer)
                .WithMany(e => e.PayrollRecords)
                .HasPrincipalKey(e => e.RpNumber)
                .HasForeignKey(e => e.RpNumber);
        });

        modelBuilder.Entity<PD7>(entity =>
        {
            entity.ToTable("PD7");

            entity.HasKey(e => new{e.RPNumber, e.Month, e.Year});

            entity.Property(e => e.RPNumber).HasColumnName("RP#").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Fee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Gross).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PD7Total).HasColumnName("PD7 Total").HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActiveEmployees).HasColumnName("Active Employees");
            entity.Property(e => e.Period).HasColumnName("Period").IsRequired();
            entity.Property(e => e.Transfer).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Received).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FeeCheck).HasColumnName("Fee Check");
            entity.Property(e => e.Month);
            entity.Property(e => e.Year);
        });
    }
}
