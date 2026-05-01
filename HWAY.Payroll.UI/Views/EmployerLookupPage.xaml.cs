using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class EmployerLookupPage : ContentPage
{
    public EmployerLookupPage(EmployerLookupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OpenWebsiteButton_Clicked(object sender, EventArgs e)
    {
        const string websiteUrl = "https://apps.cra-arc.gc.ca/ebci/rhpd/beta/entry";

        try
        {
            await Browser.Default.OpenAsync(
                websiteUrl,
                BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not open website: {ex.Message}", "OK");
        }
    }
}