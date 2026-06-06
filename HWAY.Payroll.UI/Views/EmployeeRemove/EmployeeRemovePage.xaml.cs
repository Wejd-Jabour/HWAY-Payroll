using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployeeRemovePage : ContentPage
{
    public EmployeeRemovePage(EmployeeRemoveViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}