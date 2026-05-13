using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployeeUpdatePage : ContentPage
{
    public EmployeeUpdatePage(EmployeeUpdateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}