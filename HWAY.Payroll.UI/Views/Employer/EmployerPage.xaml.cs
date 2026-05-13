using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class EmployerPage : ContentPage
{
    public EmployerPage(EmployerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}