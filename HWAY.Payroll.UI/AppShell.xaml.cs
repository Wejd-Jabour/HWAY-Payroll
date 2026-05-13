using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(PayrollWorkspacePage), typeof(PayrollWorkspacePage));
        Routing.RegisterRoute(nameof(EmployerLookupPage), typeof(EmployerLookupPage));
        Routing.RegisterRoute(nameof(PayrollRecordPage), typeof(PayrollRecordPage));

        Routing.RegisterRoute(nameof(EmployeePage), typeof(EmployeePage));
        Routing.RegisterRoute(nameof(EmployeeInsertPage), typeof(EmployeeInsertPage));
        Routing.RegisterRoute(nameof(EmployeeUpdatePage), typeof(EmployeeUpdatePage));
        Routing.RegisterRoute(nameof(EmployeeSearchPage), typeof(EmployeeSearchPage));
    }
}