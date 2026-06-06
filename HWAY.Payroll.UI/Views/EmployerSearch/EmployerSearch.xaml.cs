using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class EmployerSearchPage : ContentPage
{
    public EmployerSearchPage(EmployerSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}