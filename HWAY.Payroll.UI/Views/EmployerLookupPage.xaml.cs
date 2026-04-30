using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class EmployerLookupPage : ContentPage
{
    public EmployerLookupPage(EmployerLookupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}