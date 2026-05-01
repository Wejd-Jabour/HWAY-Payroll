using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(EmployerLookupPage), typeof(EmployerLookupPage));
    }
}