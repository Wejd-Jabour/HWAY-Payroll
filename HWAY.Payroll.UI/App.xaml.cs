using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class App : Application
{
    public App(EmployerLookupPage employerLookupPage)
    {
        InitializeComponent();

        MainPage = new NavigationPage(employerLookupPage);
    }
}