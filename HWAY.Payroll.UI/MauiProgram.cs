using Microsoft.Extensions.Logging;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayrollRecord;
using HWAY.Payroll.Services.Paystub;
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
                @"Server=tcp:hway.database.windows.net,1433;
                Initial Catalog=HWay_Payroll;
                Persist Security Info=False;
                User ID=jabourw;
                Password=HwayPayroll1;
                MultipleActiveResultSets=False;
                Encrypt=True;
                TrustServerCertificate=False;
                Connection Timeout=30;");
        });

        builder.Services.AddScoped<IEmployerLookupService, EmployerLookupService>();
        builder.Services.AddScoped<IPayrollRecordLookupService, PayrollRecordLookupService>();
        builder.Services.AddScoped<IPaystubPDFService, PaystubPDFService>();

        builder.Services.AddTransient<EmployerLookupViewModel>();
        builder.Services.AddTransient<EmployerLookupPage>();
        builder.Services.AddTransient<PayrollRecordViewModel>();
        builder.Services.AddTransient<PayrollRecordPage>();

        builder.Services.AddTransient<PayrollWorkspaceViewModel>();
        builder.Services.AddTransient<PayrollWorkspacePage>();

        // Temporary paystub preview page
        builder.Services.AddTransient<PaystubPreviewPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
