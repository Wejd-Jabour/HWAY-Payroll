using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployerInsertPage : ContentPage
{
    public EmployerInsertPage(EmployerInsertViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}