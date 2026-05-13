using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployeeSearchPage : ContentPage
{
    public EmployeeSearchPage(EmployeeSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}