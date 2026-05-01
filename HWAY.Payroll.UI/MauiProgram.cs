using Microsoft.Extensions.Logging;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.UI.ViewModels;
using HWAY.Payroll.UI.Views;
namespace HWAY.Payroll.UI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=HWAYPayrollLocalDb;Trusted_Connection=True;TrustServerCertificate=True;");
        });

        builder.Services.AddScoped<IEmployerLookupService, EmployerLookupService>();

        builder.Services.AddTransient<EmployerLookupViewModel>();
        builder.Services.AddTransient<EmployerLookupPage>();

        // Temporary paystub preview page
        builder.Services.AddTransient<PaystubPreviewPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
