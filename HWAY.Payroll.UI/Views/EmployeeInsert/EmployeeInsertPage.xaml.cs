using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployeeInsertPage : ContentPage
{
    public EmployeeInsertPage(EmployeeInsertViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}