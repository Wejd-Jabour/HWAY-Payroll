using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployerUpdatePage : ContentPage
{
    public EmployerUpdatePage(EmployerUpdateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}