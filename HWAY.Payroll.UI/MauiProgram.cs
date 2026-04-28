using Microsoft.Extensions.Logging;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

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
		options.UseSqlServer(
        @"Server=(localdb)\MSSQLLocalDB;Database=HWAYPayrollLocalDb;Trusted_Connection=True;TrustServerCertificate=True;"));

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
