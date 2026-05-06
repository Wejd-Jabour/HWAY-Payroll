using HWAY.Payroll.Services.Paystub;
using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollWorkspacePage : ContentPage
{
    private readonly IPaystubPDFService _paystubPdfService;
    private readonly PayrollWorkspaceViewModel _viewModel;

    public PayrollWorkspacePage(
        PayrollWorkspaceViewModel viewModel,
        IPaystubPDFService paystubPdfService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _paystubPdfService = paystubPdfService;

        BindingContext = _viewModel;
    }

    private async void PrintPaystubButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedPayrollRecord is null)
            return;

        if (_viewModel.PreviewPaystub is null)
        {
            _viewModel.PreviewPaystubCommand.Execute(null);
            await Task.Delay(150);
        }

        var employeeName = MakeSafeFileNamePart(_viewModel.SelectedPayrollRecord.EmployeeName);
        var currentDate = DateTime.Today.ToString("yyyy-MM-dd");

        var fileName = $"{employeeName}_{currentDate}.pdf";

        await _paystubPdfService.ExportFromViewAsync(PaystubPreviewView, fileName);
    }

    private static string MakeSafeFileNamePart(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value.Trim();
    }
}