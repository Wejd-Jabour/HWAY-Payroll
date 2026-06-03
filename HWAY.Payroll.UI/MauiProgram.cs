using Microsoft.Extensions.Logging;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayrollRecord;
using HWAY.Payroll.Services.PayrollRecordEdit;
using HWAY.Payroll.Services.Paystub;
using HWAY.Payroll.Services.EmployeeInsert;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.Services.EmployeeRemove;
using HWAY.Payroll.Services.EmployerRemove;
using HWAY.Payroll.Services.EmployerInsert;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.Services.PD7;
using HWAY.Payroll.UI.ViewModels;
using HWAY.Payroll.UI.Views;
using HWAY.Payroll.UI.ViewModel;
namespace HWAY.Payroll.UI;
using QuestPDF.Infrastructure;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{

        QuestPDF.Settings.License = LicenseType.Community;


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
            DatabaseConfiguration.ConfigureSupabase(options);
        });

        builder.Services.AddScoped<IEmployerLookupService, EmployerLookupService>();
        builder.Services.AddScoped<IPayrollRecordLookupService, PayrollRecordLookupService>();
        builder.Services.AddScoped<IPayrollRecordEditService, PayrollRecordEditService>();
        builder.Services.AddScoped<IPaystubPDFService, PaystubPDFService>();
        builder.Services.AddScoped<IEmployeeInsertService, EmployeeInsertService>();
        builder.Services.AddScoped<IEmployeeUpdateService, EmployeeUpdateService>();
        builder.Services.AddScoped<IEmployeeRemoveService, EmployeeRemoveService>();
        builder.Services.AddScoped<IEmployerInsertService, EmployerInsertService>();
        builder.Services.AddScoped<IEmployerRemoveService, EmployerRemoveService>();
        builder.Services.AddScoped<IEmployerUpdateService, EmployerUpdateService>();

        // PD7 lookup service
        builder.Services.AddScoped<IPD7LookupService, PD7LookupService>();

        builder.Services.AddTransient<EmployerLookupViewModel>();
        builder.Services.AddTransient<EmployerLookupPage>();
        builder.Services.AddTransient<PayrollRecordViewModel>();
        builder.Services.AddTransient<PayrollRecordPage>();
        builder.Services.AddTransient<PayrollRecordEditViewModel>();
        builder.Services.AddTransient<PayrollRecordEditPage>();

        builder.Services.AddTransient<PayrollWorkspaceViewModel>();
        builder.Services.AddTransient<PayrollWorkspacePage>();

        // Temporary paystub preview page
        builder.Services.AddTransient<PaystubPreviewPage>();

        builder.Services.AddTransient<EmployeeWorkspaceViewModel>();
        builder.Services.AddTransient<EmployeePage>();

        builder.Services.AddTransient<EmployerPage>();
        builder.Services.AddTransient<EmployerViewModel>();

        builder.Services.AddTransient<EmployeeInsertViewModel>();
        builder.Services.AddTransient<EmployeeInsertPage>();
        builder.Services.AddScoped<IPaystubPDFService, PaystubPDFService>();

        builder.Services.AddTransient<EmployeeUpdateViewModel>();
        builder.Services.AddTransient<EmployeeUpdatePage>();

        builder.Services.AddTransient<EmployeeSearchViewModel>();
        builder.Services.AddTransient<EmployeeSearchPage>();

        builder.Services.AddTransient<EmployeeRemoveViewModel>();
        builder.Services.AddTransient<EmployeeRemovePage>();

        builder.Services.AddTransient<EmployerInsertViewModel>();
        builder.Services.AddTransient<EmployerInsertPage>();

        builder.Services.AddTransient<EmployerRemoveViewModel>();
        builder.Services.AddTransient<EmployerRemovePage>();

        builder.Services.AddTransient<EmployerUpdateViewModel>();
        builder.Services.AddTransient<EmployerUpdatePage>();

        builder.Services.AddTransient<EmployerSearchViewModel>();
        builder.Services.AddTransient<EmployerSearchPage>();

        // PD7 view
        builder.Services.AddTransient<PD7ViewModel>();
        builder.Services.AddTransient<PD7Page>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
