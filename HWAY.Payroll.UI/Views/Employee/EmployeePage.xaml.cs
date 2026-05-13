using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class EmployeePage : ContentPage
{
    public EmployeePage(EmployeeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}