using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(PayrollWorkspacePage), typeof(PayrollWorkspacePage));

        Routing.RegisterRoute(nameof(EmployeePage), typeof(EmployeePage));
        Routing.RegisterRoute(nameof(EmployeeInsertPage), typeof(EmployeeInsertPage));
        Routing.RegisterRoute(nameof(EmployeeUpdatePage), typeof(EmployeeUpdatePage));
        Routing.RegisterRoute(nameof(EmployeeSearchPage), typeof(EmployeeSearchPage));
        Routing.RegisterRoute(nameof(EmployeeRemovePage), typeof(EmployeeRemovePage));
        Routing.RegisterRoute(nameof(EmployerInsertPage), typeof(EmployerInsertPage));
        Routing.RegisterRoute(nameof(EmployerUpdatePage), typeof(EmployerUpdatePage));
        Routing.RegisterRoute(nameof(EmployerRemovePage), typeof(EmployerRemovePage));
        Routing.RegisterRoute(nameof(EmployerSearchPage), typeof(EmployerSearchPage));
        Routing.RegisterRoute(nameof(PD7Page), typeof(PD7Page));
        Routing.RegisterRoute(nameof(T4Page), typeof(T4Page));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
