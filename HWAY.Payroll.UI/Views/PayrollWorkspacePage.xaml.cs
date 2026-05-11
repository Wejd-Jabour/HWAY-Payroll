using HWAY.Payroll.Services.Paystub;
using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollWorkspacePage : ContentPage
{
    private readonly PayrollWorkspaceViewModel _viewModel;

    public PayrollWorkspacePage(
        PayrollWorkspaceViewModel viewModel,
        IPaystubPDFService paystubPdfService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
