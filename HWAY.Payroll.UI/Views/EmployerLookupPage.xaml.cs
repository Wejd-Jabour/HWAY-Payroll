using HWAY.Payroll.UI.ViewModels;
using HWAY.Payroll.Services.Paystub;

namespace HWAY.Payroll.UI.Views;

public partial class EmployerLookupPage : ContentPage
{
    private readonly EmployerLookupViewModel _viewModel;
    private readonly IPaystubPDFService _pdfService;

    public EmployerLookupPage(EmployerLookupViewModel viewModel, IPaystubPDFService pdfService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _pdfService = pdfService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
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

    //private async void ExportPaystubButton_Clicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        await _pdfService.ExportFromViewAsync(PaystubView);
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", $"Failed to export PDF: {ex.Message}", "OK");
    //    }
    //}
}
