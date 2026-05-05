using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollWorkspacePage : ContentPage
{
    public PayrollWorkspacePage(PayrollWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}