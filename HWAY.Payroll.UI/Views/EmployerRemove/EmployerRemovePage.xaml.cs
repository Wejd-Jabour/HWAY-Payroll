using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployerRemovePage : ContentPage
{
    public EmployerRemovePage(EmployerRemoveViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}