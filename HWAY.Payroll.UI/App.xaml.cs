using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI;

public partial class App : Application
{
    public App(PaystubPreviewPage paystubPreviewPage)
    {
        InitializeComponent();

        MainPage = new NavigationPage(paystubPreviewPage);
    }
}