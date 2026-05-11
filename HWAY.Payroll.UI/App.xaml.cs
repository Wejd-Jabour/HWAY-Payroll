using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}