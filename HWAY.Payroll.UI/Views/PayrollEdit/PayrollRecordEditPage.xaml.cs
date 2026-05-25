using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class PayrollRecordEditPage : ContentPage
{
    public PayrollRecordEditPage(PayrollRecordEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}